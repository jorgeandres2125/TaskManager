using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Añade Tasks.Number: número correlativo, obligatorio y único, generado por la secuencia dbo.TaskNumberSequence.
    ///
    /// Las tareas existentes conservan el número que ya mostraban en el título ("Probar el flujo (#24)" → Number 24)
    /// y el sufijo " (#N)" se quita del título. Las que no tenían número se numeran por fecha de creación
    /// (si esos números chocaran con los del título, se colocan después del mayor). La secuencia continúa
    /// a partir del mayor número asignado.
    /// </summary>
    public partial class AddTaskNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "dbo");

            migrationBuilder.CreateSequence(name: "TaskNumberSequence", schema: "dbo");

            migrationBuilder.SqlInOwnBatch("ALTER TABLE dbo.Tasks ADD Number BIGINT NULL;");

            migrationBuilder.SqlInOwnBatch("""
                DECLARE @Numbers TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY, Number BIGINT NOT NULL);

                -- Números que ya aparecen en el título: "... (#24)".
                INSERT INTO @Numbers (Id, Number)
                SELECT t.Id,
                       TRY_CAST(SUBSTRING(t.Title, CHARINDEX(N' (#', t.Title) + 3,
                                          LEN(t.Title) - CHARINDEX(N' (#', t.Title) - 3) AS BIGINT)
                FROM dbo.Tasks AS t
                WHERE t.Title LIKE N'% (#[0-9]%)'
                  AND TRY_CAST(SUBSTRING(t.Title, CHARINDEX(N' (#', t.Title) + 3,
                                         LEN(t.Title) - CHARINDEX(N' (#', t.Title) - 3) AS BIGINT) IS NOT NULL;

                -- Si hubiera números repetidos en los títulos, no se usan: todas se numeran por fecha.
                IF EXISTS (SELECT Number FROM @Numbers GROUP BY Number HAVING COUNT(*) > 1)
                    DELETE FROM @Numbers;

                -- El resto, por fecha de creación; después del mayor número si hubiera choques.
                DECLARE @Offset BIGINT = 0;
                IF EXISTS (
                    SELECT 1
                    FROM (SELECT ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS Number
                          FROM dbo.Tasks
                          WHERE Id NOT IN (SELECT Id FROM @Numbers)) AS pending
                    WHERE pending.Number IN (SELECT Number FROM @Numbers))
                    SELECT @Offset = MAX(Number) FROM @Numbers;

                INSERT INTO @Numbers (Id, Number)
                SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) + @Offset
                FROM dbo.Tasks
                WHERE Id NOT IN (SELECT Id FROM @Numbers);

                UPDATE t
                SET t.Number = n.Number,
                    t.Title = CASE WHEN t.Title LIKE N'% (#[0-9]%)'
                                   THEN LEFT(t.Title, CHARINDEX(N' (#', t.Title) - 1)
                                   ELSE t.Title END
                FROM dbo.Tasks AS t
                INNER JOIN @Numbers AS n ON n.Id = t.Id;
                """);

            migrationBuilder.SqlInOwnBatch("ALTER TABLE dbo.Tasks ALTER COLUMN Number BIGINT NOT NULL;");
            migrationBuilder.SqlInOwnBatch("ALTER TABLE dbo.Tasks ADD CONSTRAINT DF_Tasks_Number DEFAULT (NEXT VALUE FOR dbo.TaskNumberSequence) FOR Number;");

            migrationBuilder.SqlInOwnBatch("""
                DECLARE @Next BIGINT = (SELECT ISNULL(MAX(Number), 0) + 1 FROM dbo.Tasks);
                DECLARE @Sql NVARCHAR(200) = N'ALTER SEQUENCE dbo.TaskNumberSequence RESTART WITH ' + CAST(@Next AS NVARCHAR(20)) + N';';
                EXEC sys.sp_executesql @Sql;
                """);

            migrationBuilder.CreateIndex(
                name: "UX_Tasks_Number",
                table: "Tasks",
                column: "Number",
                unique: true);

            migrationBuilder.SqlInOwnBatch(GetTasksProcedure(includeNumber: true));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch(GetTasksProcedure(includeNumber: false));

            migrationBuilder.DropIndex(name: "UX_Tasks_Number", table: "Tasks");
            migrationBuilder.SqlInOwnBatch("ALTER TABLE dbo.Tasks DROP CONSTRAINT DF_Tasks_Number;");
            migrationBuilder.DropColumn(name: "Number", table: "Tasks");
            migrationBuilder.DropSequence(name: "TaskNumberSequence", schema: "dbo");
        }

        internal static string GetTasksProcedure(bool includeNumber, bool partialTagMatch = false) => $"""
            CREATE OR ALTER PROCEDURE dbo.usp_GetTasks
                @TaskId         UNIQUEIDENTIFIER = NULL,
                @UserId         UNIQUEIDENTIFIER = NULL,
                @Status         NVARCHAR(20)     = NULL,
                @Priority       NVARCHAR(10)     = NULL,
                @Tag            NVARCHAR(50)     = NULL,
                @SortDescending BIT              = 1,
                @PageNumber     INT              = NULL,   -- desde 1; NULL equivale a 1
                @PageSize       INT              = NULL    -- NULL: sin paginar
            AS
            BEGIN
                SET NOCOUNT ON;
                {(partialTagMatch ? TagPatternDeclaration : string.Empty)}
                -- 1. Claves de las tareas que cumplen los filtros. Se guardan en una tabla temporal para
                --    contar y paginar sin repetir el WHERE.
                SELECT t.Id, t.CreatedAt
                INTO #Filtered
                FROM dbo.Tasks AS t
                WHERE (@TaskId   IS NULL OR t.Id = @TaskId)
                  AND (@UserId   IS NULL OR t.UserId = @UserId)
                  AND (@Status   IS NULL OR t.Status = @Status)
                  AND (@Priority IS NULL OR t.Priority = @Priority)                -- columna calculada con JSON_VALUE
                  AND (@Tag      IS NULL OR EXISTS (
                          SELECT 1
                          FROM OPENJSON(t.AdditionalInfo, '$.tags') AS tag
                          WHERE {(partialTagMatch ? "tag.[value] LIKE @TagPattern ESCAPE N'!'" : "tag.[value] = @Tag")}))
                -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
                -- y permite usar los índices por usuario, estado o prioridad.
                OPTION (RECOMPILE);

                -- 2. Total de tareas filtradas, para calcular las páginas.
                SELECT COUNT(*) AS TotalCount FROM #Filtered;

                -- 3. Página pedida, ordenada por fecha de creación.
                SELECT
                    t.Id,{(includeNumber ? "\n        t.Number," : string.Empty)}
                    t.Title,
                    t.Status,
                    t.UserId,
                    u.Name AS UserName,
                    t.CreatedAt,
                    t.UpdatedAt,
                    t.Priority,
                    TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, '$.estimatedCompletionDate')) AS EstimatedCompletionDate,
                    JSON_QUERY(t.AdditionalInfo, '$.tags')     AS Tags,                           -- arreglo JSON
                    JSON_QUERY(t.AdditionalInfo, '$.metadata') AS Metadata                        -- objeto JSON
                FROM #Filtered AS f
                INNER JOIN dbo.Tasks AS t ON t.Id = f.Id
                INNER JOIN dbo.Users AS u ON u.Id = t.UserId
                ORDER BY
                    CASE WHEN @SortDescending = 1 THEN f.CreatedAt END DESC,
                    CASE WHEN @SortDescending = 0 THEN f.CreatedAt END ASC,
                    f.Id                                                                  -- desempate estable entre páginas
                OFFSET ISNULL((ISNULL(@PageNumber, 1) - 1) * @PageSize, 0) ROWS
                FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
            END
            """;

        private const string TagPatternDeclaration = """

                -- Búsqueda parcial de etiqueta: "back" encuentra "backend". Los comodines se escapan para buscar el texto literal.
                DECLARE @TagPattern NVARCHAR(210) = N'%' + REPLACE(REPLACE(REPLACE(REPLACE(
                    @Tag, N'!', N'!!'), N'%', N'!%'), N'_', N'!_'), N'[', N'![') + N'%';

            """;
    }
}

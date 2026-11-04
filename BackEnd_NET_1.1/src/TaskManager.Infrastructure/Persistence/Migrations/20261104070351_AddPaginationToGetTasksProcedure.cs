using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Añade paginación a dbo.usp_GetTasks (@PageNumber, @PageSize). El procedimiento devuelve dos conjuntos de resultados:
    /// el total de tareas que cumplen los filtros y la página pedida. Sin @PageSize devuelve todas las tareas filtradas.
    /// </summary>
    public partial class AddPaginationToGetTasksProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("""
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
                              WHERE tag.[value] = @Tag))
                    -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
                    -- y permite usar los índices por usuario, estado o prioridad.
                    OPTION (RECOMPILE);

                    -- 2. Total de tareas filtradas, para calcular las páginas.
                    SELECT COUNT(*) AS TotalCount FROM #Filtered;

                    -- 3. Página pedida, ordenada por fecha de creación.
                    SELECT
                        t.Id,
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
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("""
                CREATE OR ALTER PROCEDURE dbo.usp_GetTasks
                    @TaskId         UNIQUEIDENTIFIER = NULL,
                    @UserId         UNIQUEIDENTIFIER = NULL,
                    @Status         NVARCHAR(20)     = NULL,
                    @Priority       NVARCHAR(10)     = NULL,
                    @Tag            NVARCHAR(50)     = NULL,
                    @SortDescending BIT              = 1
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        t.Id,
                        t.Title,
                        t.Status,
                        t.UserId,
                        u.Name AS UserName,
                        t.CreatedAt,
                        t.UpdatedAt,
                        t.Priority,
                        TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, '$.estimatedCompletionDate')) AS EstimatedCompletionDate,
                        JSON_QUERY(t.AdditionalInfo, '$.tags')     AS Tags,
                        JSON_QUERY(t.AdditionalInfo, '$.metadata') AS Metadata
                    FROM dbo.Tasks AS t
                    INNER JOIN dbo.Users AS u ON u.Id = t.UserId
                    WHERE (@TaskId   IS NULL OR t.Id = @TaskId)
                      AND (@UserId   IS NULL OR t.UserId = @UserId)
                      AND (@Status   IS NULL OR t.Status = @Status)
                      AND (@Priority IS NULL OR t.Priority = @Priority)
                      AND (@Tag      IS NULL OR EXISTS (
                              SELECT 1
                              FROM OPENJSON(t.AdditionalInfo, '$.tags') AS tag
                              WHERE tag.[value] = @Tag))
                    ORDER BY
                        CASE WHEN @SortDescending = 1 THEN t.CreatedAt END DESC,
                        CASE WHEN @SortDescending = 0 THEN t.CreatedAt END ASC,
                        t.Id
                    OPTION (RECOMPILE);
                END
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Consulta de tareas requerida por el enunciado: por usuario, filtrada por estado y ordenada por fecha de creación.
    /// También filtra y proyecta campos del JSON de AdditionalInfo con funciones nativas de SQL Server.
    /// </summary>
    public partial class AddGetTasksProcedure : Migration
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
                        t.Priority,                                                                    -- columna calculada con JSON_VALUE
                        TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, '$.estimatedCompletionDate')) AS EstimatedCompletionDate,
                        JSON_QUERY(t.AdditionalInfo, '$.tags')     AS Tags,                           -- arreglo JSON
                        JSON_QUERY(t.AdditionalInfo, '$.metadata') AS Metadata                        -- objeto JSON
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
                    -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
                    -- y permite usar los índices por usuario, estado o prioridad.
                    OPTION (RECOMPILE);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("DROP PROCEDURE IF EXISTS dbo.usp_GetTasks;");
        }
    }
}

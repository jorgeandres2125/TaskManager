using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// El filtro por etiqueta de dbo.usp_GetTasks pasa de coincidencia exacta a parcial: "back" encuentra "backend".
    /// Sigue usando OPENJSON sobre $.tags; los comodines de LIKE del texto buscado se escapan.
    /// </summary>
    public partial class PartialTagFilterInGetTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch(AddTaskNumber.GetTasksProcedure(includeNumber: true, partialTagMatch: true));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch(AddTaskNumber.GetTasksProcedure(includeNumber: true));
        }
    }
}

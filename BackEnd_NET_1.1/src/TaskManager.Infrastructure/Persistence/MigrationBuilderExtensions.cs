using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>Ayudas para el SQL propio de las migraciones.</summary>
internal static class MigrationBuilderExtensions
{
    public static void SqlInOwnBatch(this MigrationBuilder migrationBuilder, string sql) =>
        migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");
}

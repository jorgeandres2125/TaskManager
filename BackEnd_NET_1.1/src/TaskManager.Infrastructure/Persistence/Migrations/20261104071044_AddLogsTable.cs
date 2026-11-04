using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Tabla de logs que escribe Serilog (Serilog.Sinks.MSSqlServer). No es una entidad del dominio, por eso no está
    /// en el DbContext, pero sigue la convención del modelo: Id GUID (lo genera la base de datos con NEWSEQUENTIALID,
    /// porque el sink no envía Id) y CreatedAt obligatorio con el índice clustered. Cada evento completo se guarda
    /// en JSON en la columna LogEvent.
    /// </summary>
    public partial class AddLogsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("""
                CREATE TABLE dbo.Logs
                (
                    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Logs_Id DEFAULT NEWSEQUENTIALID(),
                    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Logs_CreatedAt DEFAULT SYSUTCDATETIME(),
                    Level     NVARCHAR(16)     NOT NULL,
                    Message   NVARCHAR(MAX)    NULL,
                    Exception NVARCHAR(MAX)    NULL,
                    LogEvent  NVARCHAR(MAX)    NULL,
                    CONSTRAINT PK_Logs PRIMARY KEY NONCLUSTERED (Id),
                    CONSTRAINT CK_Logs_LogEvent_IsJson CHECK (LogEvent IS NULL OR ISJSON(LogEvent, OBJECT) = 1)
                );
                """);

            migrationBuilder.SqlInOwnBatch("CREATE CLUSTERED INDEX CIX_Logs_CreatedAt ON dbo.Logs (CreatedAt);");
            migrationBuilder.SqlInOwnBatch("CREATE NONCLUSTERED INDEX IX_Logs_Level_CreatedAt ON dbo.Logs (Level, CreatedAt DESC);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("DROP TABLE IF EXISTS dbo.Logs;");
        }
    }
}

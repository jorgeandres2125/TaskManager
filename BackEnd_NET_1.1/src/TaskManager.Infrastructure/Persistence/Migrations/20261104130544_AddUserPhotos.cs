using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Tabla dbo.UserPhotos: la foto de cada usuario en VARBINARY(MAX) (el BLOB de SQL Server), con el nombre
    /// del archivo, su extensión, el tipo MIME y el tamaño. Una foto por usuario (UX_UserPhotos_UserId).
    /// </summary>
    public partial class AddUserPhotos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhotos", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.CheckConstraint("CK_UserPhotos_Extension_Format", "[Extension] LIKE N'.%'");
                    table.CheckConstraint("CK_UserPhotos_FileName_NotBlank", "LEN(TRIM([FileName])) > 0");
                    table.CheckConstraint("CK_UserPhotos_SizeBytes", "[SizeBytes] > 0 AND [SizeBytes] <= 2097152");
                    table.CheckConstraint("CK_UserPhotos_SizeBytes_MatchesContent", "[SizeBytes] = DATALENGTH([Content])");
                    table.ForeignKey(
                        name: "FK_UserPhotos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "CIX_UserPhotos_CreatedAt",
                table: "UserPhotos",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "UX_UserPhotos_UserId",
                table: "UserPhotos",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPhotos");
        }
    }
}

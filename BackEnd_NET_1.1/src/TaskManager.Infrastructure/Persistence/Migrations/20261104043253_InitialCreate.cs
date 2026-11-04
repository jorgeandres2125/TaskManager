using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.CheckConstraint("CK_Users_Email_NotBlank", "LEN(TRIM([Email])) > 0");
                    table.CheckConstraint("CK_Users_Name_NotBlank", "LEN(TRIM([Name])) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, computedColumnSql: "CAST(JSON_VALUE([AdditionalInfo], '$.priority') AS nvarchar(10))"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.CheckConstraint("CK_Tasks_AdditionalInfo_IsJson", "[AdditionalInfo] IS NULL OR ISJSON([AdditionalInfo], OBJECT) = 1");
                    table.CheckConstraint("CK_Tasks_AdditionalInfo_Priority", "JSON_VALUE([AdditionalInfo], '$.priority') IS NULL OR JSON_VALUE([AdditionalInfo], '$.priority') IN (N'Low', N'Medium', N'High')");
                    table.CheckConstraint("CK_Tasks_Status", "[Status] IN (N'Pending', N'InProgress', N'Done')");
                    table.CheckConstraint("CK_Tasks_Title_NotBlank", "LEN(TRIM([Title])) > 0");
                    table.ForeignKey(
                        name: "FK_Tasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "CIX_Tasks_CreatedAt",
                table: "Tasks",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status_CreatedAt",
                table: "Tasks",
                columns: new[] { "Status", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_Status_CreatedAt",
                table: "Tasks",
                columns: new[] { "UserId", "Status", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "CIX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

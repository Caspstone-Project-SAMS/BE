using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Renametotablemoduleactivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityHistory");

            migrationBuilder.CreateTable(
                name: "ModuleActivity",
                columns: table => new
                {
                    ActivityHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreparationTaskID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleActivity", x => x.ActivityHistoryId);
                    table.ForeignKey(
                        name: "FK_ModuleActivity_Module_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "Module",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleActivity_PreparationTask_PreparationTaskID",
                        column: x => x.PreparationTaskID,
                        principalTable: "PreparationTask",
                        principalColumn: "PreparationTaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleActivity_ModuleID",
                table: "ModuleActivity",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleActivity_PreparationTaskID",
                table: "ModuleActivity",
                column: "PreparationTaskID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleActivity");

            migrationBuilder.CreateTable(
                name: "ActivityHistory",
                columns: table => new
                {
                    ActivityHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    PreparationTaskID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityHistory", x => x.ActivityHistoryId);
                    table.ForeignKey(
                        name: "FK_ActivityHistory_Module_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "Module",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityHistory_PreparationTask_PreparationTaskID",
                        column: x => x.PreparationTaskID,
                        principalTable: "PreparationTask",
                        principalColumn: "PreparationTaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_ModuleID",
                table: "ActivityHistory",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_PreparationTaskID",
                table: "ActivityHistory",
                column: "PreparationTaskID");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Addactivitydescriptionofmodule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityCategory",
                columns: table => new
                {
                    ActivityCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCategory", x => x.ActivityCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "PreparationTask",
                columns: table => new
                {
                    PreparationTaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Progress = table.Column<float>(type: "real", nullable: false),
                    PreparedScheduleId = table.Column<int>(type: "int", nullable: true),
                    PreparedSchedules = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationTask", x => x.PreparationTaskID);
                });

            migrationBuilder.CreateTable(
                name: "ActivityHistory",
                columns: table => new
                {
                    ActivityHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreparationTaskID = table.Column<int>(type: "int", nullable: false),
                    ActivityCategoryID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityHistory", x => x.ActivityHistoryId);
                    table.ForeignKey(
                        name: "FK_ActivityHistory_ActivityCategory_ActivityCategoryID",
                        column: x => x.ActivityCategoryID,
                        principalTable: "ActivityCategory",
                        principalColumn: "ActivityCategoryID",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_ActivityHistory_ActivityCategoryID",
                table: "ActivityHistory",
                column: "ActivityCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_ModuleID",
                table: "ActivityHistory",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_PreparationTaskID",
                table: "ActivityHistory",
                column: "PreparationTaskID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityHistory");

            migrationBuilder.DropTable(
                name: "ActivityCategory");

            migrationBuilder.DropTable(
                name: "PreparationTask");
        }
    }
}

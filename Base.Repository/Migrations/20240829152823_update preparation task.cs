using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class updatepreparationtask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparedSchedules",
                table: "PreparationTask");

            migrationBuilder.CreateTable(
                name: "PreparedSchedule",
                columns: table => new
                {
                    PreparationTaskID = table.Column<int>(type: "int", nullable: false),
                    ScheduleID = table.Column<int>(type: "int", nullable: false),
                    TotalFingerprints = table.Column<int>(type: "int", nullable: false),
                    UploadedFingerprints = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparedSchedule", x => new { x.ScheduleID, x.PreparationTaskID });
                    table.ForeignKey(
                        name: "FK_PreparedSchedule_PreparationTask_PreparationTaskID",
                        column: x => x.PreparationTaskID,
                        principalTable: "PreparationTask",
                        principalColumn: "PreparationTaskID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreparedSchedule_Schedule_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreparationTask_PreparedScheduleId",
                table: "PreparationTask",
                column: "PreparedScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparedSchedule_PreparationTaskID",
                table: "PreparedSchedule",
                column: "PreparationTaskID");

            migrationBuilder.AddForeignKey(
                name: "FK_PreparationTask_Schedule_PreparedScheduleId",
                table: "PreparationTask",
                column: "PreparedScheduleId",
                principalTable: "Schedule",
                principalColumn: "ScheduleID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreparationTask_Schedule_PreparedScheduleId",
                table: "PreparationTask");

            migrationBuilder.DropTable(
                name: "PreparedSchedule");

            migrationBuilder.DropIndex(
                name: "IX_PreparationTask_PreparedScheduleId",
                table: "PreparationTask");

            migrationBuilder.AddColumn<string>(
                name: "PreparedSchedules",
                table: "PreparationTask",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

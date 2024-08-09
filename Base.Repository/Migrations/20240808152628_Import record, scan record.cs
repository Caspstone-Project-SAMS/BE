using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Importrecordscanrecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetMinAfterSlot",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ResetTime",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "AutoReset",
                table: "Module",
                newName: "ConnectionSound");

            migrationBuilder.AddColumn<int>(
                name: "ImportSchedulesRecordID",
                table: "Schedule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceGracePeriodMinutes",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceSound",
                table: "Module",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceSoundDurationMs",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConnectionLifetimeMs",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConnectionSoundDurationMs",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScannedTime",
                table: "Attendance",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportSchedulesRecord",
                columns: table => new
                {
                    ImportSchedulesRecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImportReverted = table.Column<bool>(type: "bit", nullable: false),
                    IsReversible = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportSchedulesRecord", x => x.ImportSchedulesRecordID);
                    table.ForeignKey(
                        name: "FK_ImportSchedulesRecord_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_ImportSchedulesRecordID",
                table: "Schedule",
                column: "ImportSchedulesRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_ImportSchedulesRecord_UserId",
                table: "ImportSchedulesRecord",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_ImportSchedulesRecord_ImportSchedulesRecordID",
                table: "Schedule",
                column: "ImportSchedulesRecordID",
                principalTable: "ImportSchedulesRecord",
                principalColumn: "ImportSchedulesRecordID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_ImportSchedulesRecord_ImportSchedulesRecordID",
                table: "Schedule");

            migrationBuilder.DropTable(
                name: "ImportSchedulesRecord");

            migrationBuilder.DropIndex(
                name: "IX_Schedule_ImportSchedulesRecordID",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "ImportSchedulesRecordID",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "AttendanceGracePeriodMinutes",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "AttendanceSound",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "AttendanceSoundDurationMs",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ConnectionLifetimeMs",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ConnectionSoundDurationMs",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ScannedTime",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "ConnectionSound",
                table: "Module",
                newName: "AutoReset");

            migrationBuilder.AddColumn<int>(
                name: "ResetMinAfterSlot",
                table: "Module",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ResetTime",
                table: "Module",
                type: "time",
                nullable: true);
        }
    }
}

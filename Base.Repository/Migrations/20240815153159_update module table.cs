using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class updatemoduletable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparedMinBeforeSlot",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "ConnectionLifetimeMs",
                table: "Module",
                newName: "ConnectionLifeTimeSeconds");

            migrationBuilder.RenameColumn(
                name: "AttendanceGracePeriodMinutes",
                table: "Module",
                newName: "AttendanceDurationMinutes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordTimestamp",
                table: "ImportSchedulesRecord",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionLifeTimeSeconds",
                table: "Module",
                newName: "ConnectionLifetimeMs");

            migrationBuilder.RenameColumn(
                name: "AttendanceDurationMinutes",
                table: "Module",
                newName: "AttendanceGracePeriodMinutes");

            migrationBuilder.AddColumn<int>(
                name: "PreparedMinBeforeSlot",
                table: "Module",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordTimestamp",
                table: "ImportSchedulesRecord",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}

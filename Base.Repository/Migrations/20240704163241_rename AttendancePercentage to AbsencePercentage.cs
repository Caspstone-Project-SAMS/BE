using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class renameAttendancePercentagetoAbsencePercentage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AttendancePercentage",
                table: "StudentClass",
                newName: "AbsencePercentage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AbsencePercentage",
                table: "StudentClass",
                newName: "AttendancePercentage");
        }
    }
}

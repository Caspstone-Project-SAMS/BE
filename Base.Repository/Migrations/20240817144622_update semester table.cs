using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class updatesemestertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemesterDurationInDays",
                table: "SystemConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlotDurationInMins",
                table: "SystemConfiguration",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SemesterDurationInDays",
                table: "SystemConfiguration");

            migrationBuilder.DropColumn(
                name: "SlotDurationInMins",
                table: "SystemConfiguration");
        }
    }
}

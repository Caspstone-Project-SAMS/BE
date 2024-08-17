using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class updateusertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "User");
        }
    }
}

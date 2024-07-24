using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Updatemoduleactivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityHistory_ActivityCategory_ActivityCategoryID",
                table: "ActivityHistory");

            migrationBuilder.DropTable(
                name: "ActivityCategory");

            migrationBuilder.DropIndex(
                name: "IX_ActivityHistory_ActivityCategoryID",
                table: "ActivityHistory");

            migrationBuilder.DropColumn(
                name: "ActivityCategoryID",
                table: "ActivityHistory");

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparedDate",
                table: "PreparationTask",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ActivityHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ActivityHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparedDate",
                table: "PreparationTask");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ActivityHistory");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ActivityHistory");

            migrationBuilder.AddColumn<int>(
                name: "ActivityCategoryID",
                table: "ActivityHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ActivityCategory",
                columns: table => new
                {
                    ActivityCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCategory", x => x.ActivityCategoryID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHistory_ActivityCategoryID",
                table: "ActivityHistory",
                column: "ActivityCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityHistory_ActivityCategory_ActivityCategoryID",
                table: "ActivityHistory",
                column: "ActivityCategoryID",
                principalTable: "ActivityCategory",
                principalColumn: "ActivityCategoryID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

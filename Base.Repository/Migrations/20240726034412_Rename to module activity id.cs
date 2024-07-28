using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Renametomoduleactivityid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActivityHistoryId",
                table: "ModuleActivity",
                newName: "ModuleActivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModuleActivityId",
                table: "ModuleActivity",
                newName: "ActivityHistoryId");
        }
    }
}

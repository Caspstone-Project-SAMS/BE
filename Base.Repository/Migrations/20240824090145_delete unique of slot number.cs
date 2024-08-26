using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class deleteuniqueofslotnumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slot_SlotNumber",
                table: "Slot");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Slot_SlotNumber",
                table: "Slot",
                column: "SlotNumber",
                unique: true);
        }
    }
}

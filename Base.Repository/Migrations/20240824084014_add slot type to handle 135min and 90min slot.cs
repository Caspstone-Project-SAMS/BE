using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class addslottypetohandle135minand90minslot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotTypeId",
                table: "Slot",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlotTypeId",
                table: "Class",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SlotType",
                columns: table => new
                {
                    SlotTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SlotDurationInMins = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotType", x => x.SlotTypeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slot_SlotTypeId",
                table: "Slot",
                column: "SlotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_SlotTypeId",
                table: "Class",
                column: "SlotTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_SlotType_SlotTypeId",
                table: "Class",
                column: "SlotTypeId",
                principalTable: "SlotType",
                principalColumn: "SlotTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Slot_SlotType_SlotTypeId",
                table: "Slot",
                column: "SlotTypeId",
                principalTable: "SlotType",
                principalColumn: "SlotTypeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_SlotType_SlotTypeId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Slot_SlotType_SlotTypeId",
                table: "Slot");

            migrationBuilder.DropTable(
                name: "SlotType");

            migrationBuilder.DropIndex(
                name: "IX_Slot_SlotTypeId",
                table: "Slot");

            migrationBuilder.DropIndex(
                name: "IX_Class_SlotTypeId",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "SlotTypeId",
                table: "Slot");

            migrationBuilder.DropColumn(
                name: "SlotTypeId",
                table: "Class");
        }
    }
}

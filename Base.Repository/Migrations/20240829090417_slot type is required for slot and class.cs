using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class slottypeisrequiredforslotandclass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_SlotType_SlotTypeId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Slot_SlotType_SlotTypeId",
                table: "Slot");

            migrationBuilder.AlterColumn<int>(
                name: "SlotTypeId",
                table: "Slot",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SlotTypeId",
                table: "Class",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_SlotType_SlotTypeId",
                table: "Class",
                column: "SlotTypeId",
                principalTable: "SlotType",
                principalColumn: "SlotTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slot_SlotType_SlotTypeId",
                table: "Slot",
                column: "SlotTypeId",
                principalTable: "SlotType",
                principalColumn: "SlotTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_SlotType_SlotTypeId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Slot_SlotType_SlotTypeId",
                table: "Slot");

            migrationBuilder.AlterColumn<int>(
                name: "SlotTypeId",
                table: "Slot",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SlotTypeId",
                table: "Class",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class AddAbsenceRecordtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleActivity_PreparationTask_PreparationTaskID",
                table: "ModuleActivity");

            migrationBuilder.DropIndex(
                name: "IX_ModuleActivity_PreparationTaskID",
                table: "ModuleActivity");

            migrationBuilder.AlterColumn<int>(
                name: "PreparationTaskID",
                table: "ModuleActivity",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Errors",
                table: "ModuleActivity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AbsenceRecord",
                columns: table => new
                {
                    AbsenceRecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceRecord", x => x.AbsenceRecordID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleActivity_PreparationTaskID",
                table: "ModuleActivity",
                column: "PreparationTaskID",
                unique: true,
                filter: "[PreparationTaskID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleActivity_PreparationTask_PreparationTaskID",
                table: "ModuleActivity",
                column: "PreparationTaskID",
                principalTable: "PreparationTask",
                principalColumn: "PreparationTaskID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleActivity_PreparationTask_PreparationTaskID",
                table: "ModuleActivity");

            migrationBuilder.DropTable(
                name: "AbsenceRecord");

            migrationBuilder.DropIndex(
                name: "IX_ModuleActivity_PreparationTaskID",
                table: "ModuleActivity");

            migrationBuilder.AlterColumn<int>(
                name: "PreparationTaskID",
                table: "ModuleActivity",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Errors",
                table: "ModuleActivity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleActivity_PreparationTaskID",
                table: "ModuleActivity",
                column: "PreparationTaskID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleActivity_PreparationTask_PreparationTaskID",
                table: "ModuleActivity",
                column: "PreparationTaskID",
                principalTable: "PreparationTask",
                principalColumn: "PreparationTaskID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

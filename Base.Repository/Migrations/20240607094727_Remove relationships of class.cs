using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Removerelationshipsofclass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Room_ClassID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Semester_ClassID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Subject_ClassID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Class_ClassID",
                table: "Schedule");

            /*migrationBuilder.AlterColumn<int>(
                name: "ClassID",
                table: "Class",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");*/

            migrationBuilder.CreateIndex(
                name: "IX_Class_RoomID",
                table: "Class",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Class_SemesterID",
                table: "Class",
                column: "SemesterID");

            migrationBuilder.CreateIndex(
                name: "IX_Class_SubjectID",
                table: "Class",
                column: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Room_RoomID",
                table: "Class",
                column: "RoomID",
                principalTable: "Room",
                principalColumn: "RoomID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Semester_SemesterID",
                table: "Class",
                column: "SemesterID",
                principalTable: "Semester",
                principalColumn: "SemesterID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Subject_SubjectID",
                table: "Class",
                column: "SubjectID",
                principalTable: "Subject",
                principalColumn: "SubjectID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Class_ClassID",
                table: "Schedule",
                column: "ClassID",
                principalTable: "Class",
                principalColumn: "ClassID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Room_RoomID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Semester_SemesterID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Subject_SubjectID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Class_ClassID",
                table: "Schedule");

            migrationBuilder.DropIndex(
                name: "IX_Class_RoomID",
                table: "Class");

            migrationBuilder.DropIndex(
                name: "IX_Class_SemesterID",
                table: "Class");

            migrationBuilder.DropIndex(
                name: "IX_Class_SubjectID",
                table: "Class");

            /*migrationBuilder.AlterColumn<int>(
                name: "ClassID",
                table: "Class",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");*/

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Room_ClassID",
                table: "Class",
                column: "ClassID",
                principalTable: "Room",
                principalColumn: "RoomID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Semester_ClassID",
                table: "Class",
                column: "ClassID",
                principalTable: "Semester",
                principalColumn: "SemesterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Subject_ClassID",
                table: "Class",
                column: "ClassID",
                principalTable: "Subject",
                principalColumn: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Class_ClassID",
                table: "Schedule",
                column: "ClassID",
                principalTable: "Class",
                principalColumn: "ClassID");
        }
    }
}

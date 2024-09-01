using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class updatenotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleActivityId",
                table: "Notification",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleID",
                table: "Notification",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_ModuleActivityId",
                table: "Notification",
                column: "ModuleActivityId",
                unique: true,
                filter: "[ModuleActivityId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_ScheduleID",
                table: "Notification",
                column: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_ModuleActivity_ModuleActivityId",
                table: "Notification",
                column: "ModuleActivityId",
                principalTable: "ModuleActivity",
                principalColumn: "ModuleActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Schedule_ScheduleID",
                table: "Notification",
                column: "ScheduleID",
                principalTable: "Schedule",
                principalColumn: "ScheduleID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_ModuleActivity_ModuleActivityId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Schedule_ScheduleID",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_ModuleActivityId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_ScheduleID",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ModuleActivityId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ScheduleID",
                table: "Notification");
        }
    }
}

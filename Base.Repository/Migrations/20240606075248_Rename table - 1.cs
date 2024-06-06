using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Repository.Migrations
{
    public partial class Renametable1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Employees_EmployeeID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Role_RoleID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Students_StudentID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AspNetUsers_StudentID",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Schedules_ScheduleID",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_AspNetUsers_LecturerID",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Rooms_ClassID",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Semesters_ClassID",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Subjects_ClassID",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_FingerprintTemplates_Students_StudentID",
                table: "FingerprintTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Employees_EmployeeID",
                table: "Modules");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationTypes_NotificationTypeID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Classes_ClassID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Slots_SlotID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_AspNetUsers_StudentID",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_Classes_ClassID",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_SubstituteTeachings_AspNetUsers_OfficialLecturerID",
                table: "SubstituteTeachings");

            migrationBuilder.DropForeignKey(
                name: "FK_SubstituteTeachings_AspNetUsers_SubstituteLecturerID",
                table: "SubstituteTeachings");

            migrationBuilder.DropForeignKey(
                name: "FK_SubstituteTeachings_Schedules_ScheduleID",
                table: "SubstituteTeachings");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubstituteTeachings",
                table: "SubstituteTeachings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects",
                table: "Subjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Slots",
                table: "Slots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Semesters",
                table: "Semesters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rooms",
                table: "Rooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationTypes",
                table: "NotificationTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Modules",
                table: "Modules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FingerprintTemplates",
                table: "FingerprintTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classes",
                table: "Classes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "SubstituteTeachings",
                newName: "SubstituteTeaching");

            migrationBuilder.RenameTable(
                name: "Subjects",
                newName: "Subject");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "Student");

            migrationBuilder.RenameTable(
                name: "Slots",
                newName: "Slot");

            migrationBuilder.RenameTable(
                name: "Semesters",
                newName: "Semester");

            migrationBuilder.RenameTable(
                name: "Schedules",
                newName: "Schedule");

            migrationBuilder.RenameTable(
                name: "Rooms",
                newName: "Room");

            migrationBuilder.RenameTable(
                name: "NotificationTypes",
                newName: "NotificationType");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notification");

            migrationBuilder.RenameTable(
                name: "Modules",
                newName: "Module");

            migrationBuilder.RenameTable(
                name: "FingerprintTemplates",
                newName: "FingerprintTemplate");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Employee");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Class");

            migrationBuilder.RenameTable(
                name: "Attendances",
                newName: "Attendance");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "User");

            migrationBuilder.RenameIndex(
                name: "IX_SubstituteTeachings_SubstituteLecturerID",
                table: "SubstituteTeaching",
                newName: "IX_SubstituteTeaching_SubstituteLecturerID");

            migrationBuilder.RenameIndex(
                name: "IX_SubstituteTeachings_ScheduleID",
                table: "SubstituteTeaching",
                newName: "IX_SubstituteTeaching_ScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_SubstituteTeachings_OfficialLecturerID",
                table: "SubstituteTeaching",
                newName: "IX_SubstituteTeaching_OfficialLecturerID");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_SubjectCode",
                table: "Subject",
                newName: "IX_Subject_SubjectCode");

            migrationBuilder.RenameIndex(
                name: "IX_Students_StudentCode",
                table: "Student",
                newName: "IX_Student_StudentCode");

            migrationBuilder.RenameIndex(
                name: "IX_Slots_SlotNumber",
                table: "Slot",
                newName: "IX_Slot_SlotNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Semesters_SemesterCode",
                table: "Semester",
                newName: "IX_Semester_SemesterCode");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_SlotID",
                table: "Schedule",
                newName: "IX_Schedule_SlotID");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_ClassID",
                table: "Schedule",
                newName: "IX_Schedule_ClassID");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserID",
                table: "Notification",
                newName: "IX_Notification_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_NotificationTypeID",
                table: "Notification",
                newName: "IX_Notification_NotificationTypeID");

            migrationBuilder.RenameIndex(
                name: "IX_Modules_Key",
                table: "Module",
                newName: "IX_Module_Key");

            migrationBuilder.RenameIndex(
                name: "IX_Modules_EmployeeID",
                table: "Module",
                newName: "IX_Module_EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_FingerprintTemplates_StudentID",
                table: "FingerprintTemplate",
                newName: "IX_FingerprintTemplate_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_LecturerID",
                table: "Class",
                newName: "IX_Class_LecturerID");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_StudentID",
                table: "Attendance",
                newName: "IX_Attendance_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_ScheduleID",
                table: "Attendance",
                newName: "IX_Attendance_ScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_StudentID",
                table: "User",
                newName: "IX_User_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_RoleID",
                table: "User",
                newName: "IX_User_RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "User",
                newName: "IX_User_PhoneNumber");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_EmployeeID",
                table: "User",
                newName: "IX_User_EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_Email",
                table: "User",
                newName: "IX_User_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubstituteTeaching",
                table: "SubstituteTeaching",
                column: "SubstituteTeachingID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subject",
                table: "Subject",
                column: "SubjectID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                table: "Student",
                column: "StudentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Slot",
                table: "Slot",
                column: "SlotID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Semester",
                table: "Semester",
                column: "SemesterID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule",
                column: "ScheduleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Room",
                table: "Room",
                column: "RoomID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationType",
                table: "NotificationType",
                column: "NotificationTypeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                column: "NotificationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Module",
                table: "Module",
                column: "ModuleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FingerprintTemplate",
                table: "FingerprintTemplate",
                column: "FingerprintTemplateID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                table: "Employee",
                column: "EmployeeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Class",
                table: "Class",
                column: "ClassID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance",
                column: "AttendanceID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Schedule_ScheduleID",
                table: "Attendance",
                column: "ScheduleID",
                principalTable: "Schedule",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_User_StudentID",
                table: "Attendance",
                column: "StudentID",
                principalTable: "User",
                principalColumn: "Id");

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
                name: "FK_Class_User_LecturerID",
                table: "Class",
                column: "LecturerID",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FingerprintTemplate_Student_StudentID",
                table: "FingerprintTemplate",
                column: "StudentID",
                principalTable: "Student",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_Employee_EmployeeID",
                table: "Module",
                column: "EmployeeID",
                principalTable: "Employee",
                principalColumn: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_NotificationType_NotificationTypeID",
                table: "Notification",
                column: "NotificationTypeID",
                principalTable: "NotificationType",
                principalColumn: "NotificationTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_User_UserID",
                table: "Notification",
                column: "UserID",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Class_ClassID",
                table: "Schedule",
                column: "ClassID",
                principalTable: "Class",
                principalColumn: "ClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Slot_SlotID",
                table: "Schedule",
                column: "SlotID",
                principalTable: "Slot",
                principalColumn: "SlotID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_Class_ClassID",
                table: "StudentClass",
                column: "ClassID",
                principalTable: "Class",
                principalColumn: "ClassID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_User_StudentID",
                table: "StudentClass",
                column: "StudentID",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubstituteTeaching_Schedule_ScheduleID",
                table: "SubstituteTeaching",
                column: "ScheduleID",
                principalTable: "Schedule",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_SubstituteTeaching_User_OfficialLecturerID",
                table: "SubstituteTeaching",
                column: "OfficialLecturerID",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubstituteTeaching_User_SubstituteLecturerID",
                table: "SubstituteTeaching",
                column: "SubstituteLecturerID",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employee_EmployeeID",
                table: "User",
                column: "EmployeeID",
                principalTable: "Employee",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_RoleID",
                table: "User",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Student_StudentID",
                table: "User",
                column: "StudentID",
                principalTable: "Student",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Schedule_ScheduleID",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_User_StudentID",
                table: "Attendance");

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
                name: "FK_Class_User_LecturerID",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_FingerprintTemplate_Student_StudentID",
                table: "FingerprintTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_Module_Employee_EmployeeID",
                table: "Module");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_NotificationType_NotificationTypeID",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_User_UserID",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Class_ClassID",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Slot_SlotID",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_Class_ClassID",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_User_StudentID",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_SubstituteTeaching_Schedule_ScheduleID",
                table: "SubstituteTeaching");

            migrationBuilder.DropForeignKey(
                name: "FK_SubstituteTeaching_User_OfficialLecturerID",
                table: "SubstituteTeaching");

            migrationBuilder.DropForeignKey(
                name: "FK_SubstituteTeaching_User_SubstituteLecturerID",
                table: "SubstituteTeaching");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Employee_EmployeeID",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_RoleID",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Student_StudentID",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubstituteTeaching",
                table: "SubstituteTeaching");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subject",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Student",
                table: "Student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Slot",
                table: "Slot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Semester",
                table: "Semester");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Room",
                table: "Room");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationType",
                table: "NotificationType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Module",
                table: "Module");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FingerprintTemplate",
                table: "FingerprintTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                table: "Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Class",
                table: "Class");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "SubstituteTeaching",
                newName: "SubstituteTeachings");

            migrationBuilder.RenameTable(
                name: "Subject",
                newName: "Subjects");

            migrationBuilder.RenameTable(
                name: "Student",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "Slot",
                newName: "Slots");

            migrationBuilder.RenameTable(
                name: "Semester",
                newName: "Semesters");

            migrationBuilder.RenameTable(
                name: "Schedule",
                newName: "Schedules");

            migrationBuilder.RenameTable(
                name: "Room",
                newName: "Rooms");

            migrationBuilder.RenameTable(
                name: "NotificationType",
                newName: "NotificationTypes");

            migrationBuilder.RenameTable(
                name: "Notification",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "Module",
                newName: "Modules");

            migrationBuilder.RenameTable(
                name: "FingerprintTemplate",
                newName: "FingerprintTemplates");

            migrationBuilder.RenameTable(
                name: "Employee",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "Class",
                newName: "Classes");

            migrationBuilder.RenameTable(
                name: "Attendance",
                newName: "Attendances");

            migrationBuilder.RenameIndex(
                name: "IX_User_StudentID",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_User_RoleID",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_User_PhoneNumber",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_PhoneNumber");

            migrationBuilder.RenameIndex(
                name: "IX_User_EmployeeID",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_User_Email",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_Email");

            migrationBuilder.RenameIndex(
                name: "IX_SubstituteTeaching_SubstituteLecturerID",
                table: "SubstituteTeachings",
                newName: "IX_SubstituteTeachings_SubstituteLecturerID");

            migrationBuilder.RenameIndex(
                name: "IX_SubstituteTeaching_ScheduleID",
                table: "SubstituteTeachings",
                newName: "IX_SubstituteTeachings_ScheduleID");

            migrationBuilder.RenameIndex(
                name: "IX_SubstituteTeaching_OfficialLecturerID",
                table: "SubstituteTeachings",
                newName: "IX_SubstituteTeachings_OfficialLecturerID");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_SubjectCode",
                table: "Subjects",
                newName: "IX_Subjects_SubjectCode");

            migrationBuilder.RenameIndex(
                name: "IX_Student_StudentCode",
                table: "Students",
                newName: "IX_Students_StudentCode");

            migrationBuilder.RenameIndex(
                name: "IX_Slot_SlotNumber",
                table: "Slots",
                newName: "IX_Slots_SlotNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Semester_SemesterCode",
                table: "Semesters",
                newName: "IX_Semesters_SemesterCode");

            migrationBuilder.RenameIndex(
                name: "IX_Schedule_SlotID",
                table: "Schedules",
                newName: "IX_Schedules_SlotID");

            migrationBuilder.RenameIndex(
                name: "IX_Schedule_ClassID",
                table: "Schedules",
                newName: "IX_Schedules_ClassID");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_UserID",
                table: "Notifications",
                newName: "IX_Notifications_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_NotificationTypeID",
                table: "Notifications",
                newName: "IX_Notifications_NotificationTypeID");

            migrationBuilder.RenameIndex(
                name: "IX_Module_Key",
                table: "Modules",
                newName: "IX_Modules_Key");

            migrationBuilder.RenameIndex(
                name: "IX_Module_EmployeeID",
                table: "Modules",
                newName: "IX_Modules_EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_FingerprintTemplate_StudentID",
                table: "FingerprintTemplates",
                newName: "IX_FingerprintTemplates_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_Class_LecturerID",
                table: "Classes",
                newName: "IX_Classes_LecturerID");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_StudentID",
                table: "Attendances",
                newName: "IX_Attendances_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_ScheduleID",
                table: "Attendances",
                newName: "IX_Attendances_ScheduleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubstituteTeachings",
                table: "SubstituteTeachings",
                column: "SubstituteTeachingID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects",
                table: "Subjects",
                column: "SubjectID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "StudentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Slots",
                table: "Slots",
                column: "SlotID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Semesters",
                table: "Semesters",
                column: "SemesterID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules",
                column: "ScheduleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rooms",
                table: "Rooms",
                column: "RoomID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationTypes",
                table: "NotificationTypes",
                column: "NotificationTypeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "NotificationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                column: "ModuleID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FingerprintTemplates",
                table: "FingerprintTemplates",
                column: "FingerprintTemplateID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "EmployeeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classes",
                table: "Classes",
                column: "ClassID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances",
                column: "AttendanceID");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Employees_EmployeeID",
                table: "AspNetUsers",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Role_RoleID",
                table: "AspNetUsers",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Students_StudentID",
                table: "AspNetUsers",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AspNetUsers_StudentID",
                table: "Attendances",
                column: "StudentID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Schedules_ScheduleID",
                table: "Attendances",
                column: "ScheduleID",
                principalTable: "Schedules",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_AspNetUsers_LecturerID",
                table: "Classes",
                column: "LecturerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Rooms_ClassID",
                table: "Classes",
                column: "ClassID",
                principalTable: "Rooms",
                principalColumn: "RoomID");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Semesters_ClassID",
                table: "Classes",
                column: "ClassID",
                principalTable: "Semesters",
                principalColumn: "SemesterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Subjects_ClassID",
                table: "Classes",
                column: "ClassID",
                principalTable: "Subjects",
                principalColumn: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_FingerprintTemplates_Students_StudentID",
                table: "FingerprintTemplates",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Employees_EmployeeID",
                table: "Modules",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserID",
                table: "Notifications",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationTypes_NotificationTypeID",
                table: "Notifications",
                column: "NotificationTypeID",
                principalTable: "NotificationTypes",
                principalColumn: "NotificationTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Classes_ClassID",
                table: "Schedules",
                column: "ClassID",
                principalTable: "Classes",
                principalColumn: "ClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Slots_SlotID",
                table: "Schedules",
                column: "SlotID",
                principalTable: "Slots",
                principalColumn: "SlotID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_AspNetUsers_StudentID",
                table: "StudentClass",
                column: "StudentID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_Classes_ClassID",
                table: "StudentClass",
                column: "ClassID",
                principalTable: "Classes",
                principalColumn: "ClassID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubstituteTeachings_AspNetUsers_OfficialLecturerID",
                table: "SubstituteTeachings",
                column: "OfficialLecturerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubstituteTeachings_AspNetUsers_SubstituteLecturerID",
                table: "SubstituteTeachings",
                column: "SubstituteLecturerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubstituteTeachings_Schedules_ScheduleID",
                table: "SubstituteTeachings",
                column: "ScheduleID",
                principalTable: "Schedules",
                principalColumn: "ScheduleID");
        }
    }
}

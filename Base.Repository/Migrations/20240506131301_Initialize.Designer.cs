﻿// <auto-generated />
using System;
using Base.Repository.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Base.Repository.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240506131301_Initialize")]
    partial class Initialize
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Base.Repository.Entity.AttendanceReport", b =>
                {
                    b.Property<int>("AttendanceReportID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AttendanceReportID"), 1L, 1);

                    b.Property<int>("AttendanceStatus")
                        .HasColumnType("int");

                    b.Property<string>("Comments")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ScheduleID")
                        .HasColumnType("int");

                    b.Property<Guid>("StudentID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("AttendanceReportID");

                    b.HasIndex("ScheduleID");

                    b.HasIndex("StudentID");

                    b.ToTable("AttendanceReports");
                });

            modelBuilder.Entity("Base.Repository.Entity.Campus", b =>
                {
                    b.Property<int>("CampusID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CampusID"), 1L, 1);

                    b.Property<string>("CampusName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CampusID");

                    b.ToTable("Campuses");
                });

            modelBuilder.Entity("Base.Repository.Entity.Class", b =>
                {
                    b.Property<int>("ClassID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ClassID"), 1L, 1);

                    b.Property<string>("ClassCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CourseID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("LecturerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("ClassID");

                    b.HasIndex("CourseID");

                    b.HasIndex("LecturerID");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("Base.Repository.Entity.Course", b =>
                {
                    b.Property<int>("CourseID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CourseID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SemesterID")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("SubjectID")
                        .HasColumnType("int");

                    b.HasKey("CourseID");

                    b.HasIndex("SemesterID");

                    b.HasIndex("SubjectID");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("Base.Repository.Entity.Curriculum", b =>
                {
                    b.Property<int>("CurriculumID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CurriculumID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CurriculumID");

                    b.ToTable("Curricula");
                });

            modelBuilder.Entity("Base.Repository.Entity.FingerprintTemplate", b =>
                {
                    b.Property<int>("FingerprintTemplateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FingerprintTemplateID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FingerprintTemplateData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("StudentID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("FingerprintTemplateID");

                    b.HasIndex("StudentID");

                    b.ToTable("FingerprintTemplates");
                });

            modelBuilder.Entity("Base.Repository.Entity.FingerScanRecord", b =>
                {
                    b.Property<int>("FingerScanReportID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FingerScanReportID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FingerprintTemplate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MyProperty")
                        .HasColumnType("int");

                    b.Property<DateTime>("RecordTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("ScheduleID")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("FingerScanReportID");

                    b.HasIndex("ScheduleID");

                    b.ToTable("FingerScanRecords");
                });

            modelBuilder.Entity("Base.Repository.Entity.Module", b =>
                {
                    b.Property<int>("ModuleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ModuleID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Mode")
                        .HasColumnType("int");

                    b.Property<int?>("RoomID")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ModuleID");

                    b.HasIndex("RoomID")
                        .IsUnique()
                        .HasFilter("[RoomID] IS NOT NULL");

                    b.ToTable("Modules");
                });

            modelBuilder.Entity("Base.Repository.Entity.Notification", b =>
                {
                    b.Property<int>("NotificationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NotificationID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("NotificationID");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("Base.Repository.Entity.Room", b =>
                {
                    b.Property<int>("RoomID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RoomID"), 1L, 1);

                    b.Property<int>("CampusID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoomName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RoomID");

                    b.HasIndex("CampusID");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("Base.Repository.Entity.ScheduleTable", b =>
                {
                    b.Property<int>("ScheduleTableID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ScheduleTableID"), 1L, 1);

                    b.Property<int>("ClassID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<int>("DateOfWeek")
                        .HasColumnType("int");

                    b.Property<int>("RoomID")
                        .HasColumnType("int");

                    b.Property<int>("SlotID")
                        .HasColumnType("int");

                    b.HasKey("ScheduleTableID");

                    b.HasIndex("ClassID");

                    b.HasIndex("RoomID");

                    b.HasIndex("SlotID");

                    b.ToTable("ScheduleTables");
                });

            modelBuilder.Entity("Base.Repository.Entity.Semester", b =>
                {
                    b.Property<int>("SemesterID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SemesterID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("date");

                    b.Property<string>("SemesterCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("date");

                    b.HasKey("SemesterID");

                    b.ToTable("Semesters");
                });

            modelBuilder.Entity("Base.Repository.Entity.Slot", b =>
                {
                    b.Property<int>("SlotID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SlotID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<TimeSpan>("Endtime")
                        .HasColumnType("time");

                    b.Property<int>("SlotNumber")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("time");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("SlotID");

                    b.ToTable("Slots");
                });

            modelBuilder.Entity("Base.Repository.Entity.Student", b =>
                {
                    b.Property<Guid>("StudentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CurriculumID")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("NormalizedEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StudentCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StudentID");

                    b.HasIndex("CurriculumID");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("Base.Repository.Entity.Subject", b =>
                {
                    b.Property<int>("SubjectID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SubjectID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SubjectCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SubjectID");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("Base.Repository.Identity.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Identity.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FilePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActivated")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasFilter("[Email] IS NOT NULL");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("PhoneNumber")
                        .IsUnique()
                        .HasFilter("[PhoneNumber] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("CurriculumSubject", b =>
                {
                    b.Property<int>("SubjectID")
                        .HasColumnType("int");

                    b.Property<int>("CurriculumID")
                        .HasColumnType("int");

                    b.HasKey("SubjectID", "CurriculumID");

                    b.HasIndex("CurriculumID");

                    b.ToTable("CurriculumSubject");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("StudentClass", b =>
                {
                    b.Property<Guid>("StudentID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ClassID")
                        .HasColumnType("int");

                    b.HasKey("StudentID", "ClassID");

                    b.HasIndex("ClassID");

                    b.ToTable("StudentClass");
                });

            modelBuilder.Entity("Base.Repository.Entity.AttendanceReport", b =>
                {
                    b.HasOne("Base.Repository.Entity.ScheduleTable", "ScheduleTable")
                        .WithMany("AttendanceReports")
                        .HasForeignKey("ScheduleID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Student", "Student")
                        .WithMany("AttendanceReports")
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("ScheduleTable");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Base.Repository.Entity.Class", b =>
                {
                    b.HasOne("Base.Repository.Entity.Course", "Course")
                        .WithMany("Classes")
                        .HasForeignKey("CourseID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Identity.User", "Lecturer")
                        .WithMany("Classes")
                        .HasForeignKey("LecturerID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Lecturer");
                });

            modelBuilder.Entity("Base.Repository.Entity.Course", b =>
                {
                    b.HasOne("Base.Repository.Entity.Semester", "Semester")
                        .WithMany("Courses")
                        .HasForeignKey("SemesterID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Subject", "Subject")
                        .WithMany("Courses")
                        .HasForeignKey("SubjectID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Semester");

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("Base.Repository.Entity.FingerprintTemplate", b =>
                {
                    b.HasOne("Base.Repository.Entity.Student", "Student")
                        .WithMany("FingerprintTemplates")
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Base.Repository.Entity.FingerScanRecord", b =>
                {
                    b.HasOne("Base.Repository.Entity.ScheduleTable", "ScheduleTable")
                        .WithMany("FingerScanRecords")
                        .HasForeignKey("ScheduleID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("ScheduleTable");
                });

            modelBuilder.Entity("Base.Repository.Entity.Module", b =>
                {
                    b.HasOne("Base.Repository.Entity.Room", "Room")
                        .WithOne("Module")
                        .HasForeignKey("Base.Repository.Entity.Module", "RoomID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Base.Repository.Entity.Room", b =>
                {
                    b.HasOne("Base.Repository.Entity.Campus", "Campus")
                        .WithMany("Rooms")
                        .HasForeignKey("CampusID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Campus");
                });

            modelBuilder.Entity("Base.Repository.Entity.ScheduleTable", b =>
                {
                    b.HasOne("Base.Repository.Entity.Class", "Class")
                        .WithMany("ScheduleTables")
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Room", "Room")
                        .WithMany()
                        .HasForeignKey("RoomID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Slot", "Slot")
                        .WithMany("ScheduleTables")
                        .HasForeignKey("SlotID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Room");

                    b.Navigation("Slot");
                });

            modelBuilder.Entity("Base.Repository.Entity.Student", b =>
                {
                    b.HasOne("Base.Repository.Entity.Curriculum", "Curriculum")
                        .WithMany("Students")
                        .HasForeignKey("CurriculumID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Curriculum");
                });

            modelBuilder.Entity("CurriculumSubject", b =>
                {
                    b.HasOne("Base.Repository.Entity.Curriculum", null)
                        .WithMany()
                        .HasForeignKey("CurriculumID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Subject", null)
                        .WithMany()
                        .HasForeignKey("SubjectID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("Base.Repository.Identity.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("StudentClass", b =>
                {
                    b.HasOne("Base.Repository.Entity.Class", null)
                        .WithMany()
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Student", null)
                        .WithMany()
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Base.Repository.Entity.Campus", b =>
                {
                    b.Navigation("Rooms");
                });

            modelBuilder.Entity("Base.Repository.Entity.Class", b =>
                {
                    b.Navigation("ScheduleTables");
                });

            modelBuilder.Entity("Base.Repository.Entity.Course", b =>
                {
                    b.Navigation("Classes");
                });

            modelBuilder.Entity("Base.Repository.Entity.Curriculum", b =>
                {
                    b.Navigation("Students");
                });

            modelBuilder.Entity("Base.Repository.Entity.Room", b =>
                {
                    b.Navigation("Module");
                });

            modelBuilder.Entity("Base.Repository.Entity.ScheduleTable", b =>
                {
                    b.Navigation("AttendanceReports");

                    b.Navigation("FingerScanRecords");
                });

            modelBuilder.Entity("Base.Repository.Entity.Semester", b =>
                {
                    b.Navigation("Courses");
                });

            modelBuilder.Entity("Base.Repository.Entity.Slot", b =>
                {
                    b.Navigation("ScheduleTables");
                });

            modelBuilder.Entity("Base.Repository.Entity.Student", b =>
                {
                    b.Navigation("AttendanceReports");

                    b.Navigation("FingerprintTemplates");
                });

            modelBuilder.Entity("Base.Repository.Entity.Subject", b =>
                {
                    b.Navigation("Courses");
                });

            modelBuilder.Entity("Base.Repository.Identity.User", b =>
                {
                    b.Navigation("Classes");
                });
#pragma warning restore 612, 618
        }
    }
}

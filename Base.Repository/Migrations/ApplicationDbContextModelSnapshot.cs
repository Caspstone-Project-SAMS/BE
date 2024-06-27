﻿// <auto-generated />
using System;
using Base.Repository.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Base.Repository.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Base.Repository.Entity.Attendance", b =>
                {
                    b.Property<int>("AttendanceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AttendanceID"), 1L, 1);

                    b.Property<int>("AttendanceStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("AttendanceTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Comments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("ScheduleID")
                        .HasColumnType("int");

                    b.Property<Guid>("StudentID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("AttendanceID");

                    b.HasIndex("ScheduleID");

                    b.HasIndex("StudentID");

                    b.ToTable("Attendance", (string)null);
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

                    b.Property<int>("ClassStatus")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid>("LecturerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("RoomID")
                        .HasColumnType("int");

                    b.Property<int>("SemesterID")
                        .HasColumnType("int");

                    b.Property<int>("SubjectID")
                        .HasColumnType("int");

                    b.HasKey("ClassID");

                    b.HasIndex("LecturerID");

                    b.HasIndex("RoomID");

                    b.HasIndex("SemesterID");

                    b.HasIndex("SubjectID");

                    b.ToTable("Class", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Entity.Employee", b =>
                {
                    b.Property<Guid>("EmployeeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Department")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.HasKey("EmployeeID");

                    b.ToTable("Employee", (string)null);
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("StudentID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("FingerprintTemplateID");

                    b.HasIndex("StudentID");

                    b.ToTable("FingerprintTemplate", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Entity.Module", b =>
                {
                    b.Property<int>("ModuleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ModuleID"), 1L, 1);

                    b.Property<bool>("AutoPrepare")
                        .HasColumnType("bit");

                    b.Property<bool>("AutoReset")
                        .HasColumnType("bit");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("EmployeeID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Mode")
                        .HasColumnType("int");

                    b.Property<int?>("PreparedMinBeforeSlot")
                        .HasColumnType("int");

                    b.Property<TimeSpan?>("PreparedTime")
                        .HasColumnType("time");

                    b.Property<int?>("ResetMinAfterSlot")
                        .HasColumnType("int");

                    b.Property<TimeSpan?>("ResetTime")
                        .HasColumnType("time");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("ModuleID");

                    b.HasIndex("EmployeeID");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("Module", (string)null);
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("NotificationTypeID")
                        .HasColumnType("int");

                    b.Property<bool>("Read")
                        .HasColumnType("bit");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("NotificationID");

                    b.HasIndex("NotificationTypeID");

                    b.HasIndex("UserID");

                    b.ToTable("Notification", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Entity.NotificationType", b =>
                {
                    b.Property<int>("NotificationTypeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NotificationTypeID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("TypeDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NotificationTypeID");

                    b.ToTable("NotificationType", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Entity.Room", b =>
                {
                    b.Property<int>("RoomID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RoomID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("RoomDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoomName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoomStatus")
                        .HasColumnType("int");

                    b.HasKey("RoomID");

                    b.ToTable("Room", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Entity.Schedule", b =>
                {
                    b.Property<int>("ScheduleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ScheduleID"), 1L, 1);

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int?>("RoomID")
                        .HasColumnType("int");

                    b.Property<int>("ScheduleStatus")
                        .HasColumnType("int");

                    b.Property<int>("SlotID")
                        .HasColumnType("int");

                    b.HasKey("ScheduleID");

                    b.HasIndex("ClassID");

                    b.HasIndex("RoomID");

                    b.HasIndex("SlotID");

                    b.ToTable("Schedule", (string)null);
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("SemesterCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("SemesterStatus")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("date");

                    b.HasKey("SemesterID");

                    b.HasIndex("SemesterCode")
                        .IsUnique();

                    b.ToTable("Semester", (string)null);
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<int>("SlotNumber")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("time");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("SlotID");

                    b.HasIndex("SlotNumber")
                        .IsUnique();

                    b.ToTable("Slot", (string)null);
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("StudentCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("StudentID");

                    b.HasIndex("StudentCode")
                        .IsUnique();

                    b.ToTable("Student", (string)null);
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("SubjectCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SubjectName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SubjectStatus")
                        .HasColumnType("int");

                    b.HasKey("SubjectID");

                    b.HasIndex("SubjectCode")
                        .IsUnique();

                    b.ToTable("Subject", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Entity.SubstituteTeaching", b =>
                {
                    b.Property<int>("SubstituteTeachingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SubstituteTeachingID"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid>("OfficialLecturerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ScheduleID")
                        .HasColumnType("int");

                    b.Property<Guid>("SubstituteLecturerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("SubstituteTeachingStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.HasKey("SubstituteTeachingID");

                    b.HasIndex("OfficialLecturerID");

                    b.HasIndex("ScheduleID")
                        .IsUnique();

                    b.HasIndex("SubstituteLecturerID");

                    b.ToTable("SubstituteTeaching", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Identity.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RoleId"), 1L, 1);

                    b.Property<string>("ConcurrencyStamp")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RoleId");

                    b.ToTable("Role", (string)null);
                });

            modelBuilder.Entity("Base.Repository.Identity.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DOB")
                        .HasColumnType("date");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<Guid?>("EmployeeID")
                        .HasColumnType("uniqueidentifier");

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

                    b.Property<int?>("RoleID")
                        .HasColumnType("int");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("StudentID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasFilter("[Email] IS NOT NULL");

                    b.HasIndex("EmployeeID")
                        .IsUnique()
                        .HasFilter("[EmployeeID] IS NOT NULL");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("PhoneNumber")
                        .IsUnique()
                        .HasFilter("[PhoneNumber] IS NOT NULL");

                    b.HasIndex("RoleID");

                    b.HasIndex("StudentID")
                        .IsUnique()
                        .HasFilter("[StudentID] IS NOT NULL");

                    b.ToTable("User", (string)null);
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

            modelBuilder.Entity("Base.Repository.Entity.Attendance", b =>
                {
                    b.HasOne("Base.Repository.Entity.Schedule", "Schedule")
                        .WithMany("Attendances")
                        .HasForeignKey("ScheduleID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Identity.User", "Student")
                        .WithMany("Attendances")
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Schedule");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Base.Repository.Entity.Class", b =>
                {
                    b.HasOne("Base.Repository.Identity.User", "Lecturer")
                        .WithMany("ManagedClasses")
                        .HasForeignKey("LecturerID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Room", "Room")
                        .WithMany("Classes")
                        .HasForeignKey("RoomID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Semester", "Semester")
                        .WithMany("Classes")
                        .HasForeignKey("SemesterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Subject", "Subject")
                        .WithMany("Classes")
                        .HasForeignKey("SubjectID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Lecturer");

                    b.Navigation("Room");

                    b.Navigation("Semester");

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("Base.Repository.Entity.FingerprintTemplate", b =>
                {
                    b.HasOne("Base.Repository.Entity.Student", "Student")
                        .WithMany("FingerprintTemplates")
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Base.Repository.Entity.Module", b =>
                {
                    b.HasOne("Base.Repository.Entity.Employee", "Employee")
                        .WithMany("Modules")
                        .HasForeignKey("EmployeeID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("Base.Repository.Entity.Notification", b =>
                {
                    b.HasOne("Base.Repository.Entity.NotificationType", "NotificationType")
                        .WithMany("Notifications")
                        .HasForeignKey("NotificationTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Identity.User", "User")
                        .WithMany("Notifications")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NotificationType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Base.Repository.Entity.Schedule", b =>
                {
                    b.HasOne("Base.Repository.Entity.Class", "Class")
                        .WithMany("Schedules")
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Room", "Room")
                        .WithMany("Schedules")
                        .HasForeignKey("RoomID")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Base.Repository.Entity.Slot", "Slot")
                        .WithMany("Schedules")
                        .HasForeignKey("SlotID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Room");

                    b.Navigation("Slot");
                });

            modelBuilder.Entity("Base.Repository.Entity.SubstituteTeaching", b =>
                {
                    b.HasOne("Base.Repository.Identity.User", "OfficialLecturer")
                        .WithMany("AssignedTeachings")
                        .HasForeignKey("OfficialLecturerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Entity.Schedule", "Schedule")
                        .WithOne("SubstituteTeaching")
                        .HasForeignKey("Base.Repository.Entity.SubstituteTeaching", "ScheduleID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Repository.Identity.User", "SubstituteLecturer")
                        .WithMany("SubstituteTeachings")
                        .HasForeignKey("SubstituteLecturerID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("OfficialLecturer");

                    b.Navigation("Schedule");

                    b.Navigation("SubstituteLecturer");
                });

            modelBuilder.Entity("Base.Repository.Identity.User", b =>
                {
                    b.HasOne("Base.Repository.Entity.Employee", "Employee")
                        .WithOne("User")
                        .HasForeignKey("Base.Repository.Identity.User", "EmployeeID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Base.Repository.Identity.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Base.Repository.Entity.Student", "Student")
                        .WithOne("User")
                        .HasForeignKey("Base.Repository.Identity.User", "StudentID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Employee");

                    b.Navigation("Role");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("StudentClass", b =>
                {
                    b.HasOne("Base.Repository.Entity.Class", null)
                        .WithMany()
                        .HasForeignKey("ClassID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Repository.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("StudentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Base.Repository.Entity.Class", b =>
                {
                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("Base.Repository.Entity.Employee", b =>
                {
                    b.Navigation("Modules");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Base.Repository.Entity.NotificationType", b =>
                {
                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("Base.Repository.Entity.Room", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("Base.Repository.Entity.Schedule", b =>
                {
                    b.Navigation("Attendances");

                    b.Navigation("SubstituteTeaching");
                });

            modelBuilder.Entity("Base.Repository.Entity.Semester", b =>
                {
                    b.Navigation("Classes");
                });

            modelBuilder.Entity("Base.Repository.Entity.Slot", b =>
                {
                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("Base.Repository.Entity.Student", b =>
                {
                    b.Navigation("FingerprintTemplates");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Base.Repository.Entity.Subject", b =>
                {
                    b.Navigation("Classes");
                });

            modelBuilder.Entity("Base.Repository.Identity.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Base.Repository.Identity.User", b =>
                {
                    b.Navigation("AssignedTeachings");

                    b.Navigation("Attendances");

                    b.Navigation("ManagedClasses");

                    b.Navigation("Notifications");

                    b.Navigation("SubstituteTeachings");
                });
#pragma warning restore 612, 618
        }
    }
}

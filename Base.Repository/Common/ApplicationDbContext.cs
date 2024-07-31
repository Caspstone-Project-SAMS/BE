using Base.Repository.Entity;
using Base.Repository.Helper;
using Base.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Base.Repository.Common;

public interface IApplicationDbContext : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
}

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationType> NotificationTypes { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<FingerprintTemplate> FingerprintTemplates { get; set; } = null!;
    public DbSet<Class> Classes { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<Slot> Slots { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<Module> Modules { get; set; } = null!;
    public DbSet<SubstituteTeaching> SubstituteTeachings { get; set; } = null!;
    public DbSet<PreparationTask> PreparationTasks { get; set; } = null!;
    public DbSet<ModuleActivity> ActivityHistories { get; set; } = null!;




    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            base.ConfigureConventions(builder);

            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter, DateOnlyComparer>()
                .HaveColumnType("date");

            builder.Properties<TimeOnly>()
                .HaveConversion<TimeOnlyConverter, TimeOnlyComparer>();
        }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Define a custom value comparer for key-value<string,string>
        var keyValueComparer = new ValueComparer<KeyValuePair<string,string>?>(
            (c1, c2) => c1.HasValue && c2.HasValue ? JsonConvert.SerializeObject(c1.Value) == JsonConvert.SerializeObject(c2.Value) : c1.HasValue == c2.HasValue,
            c => c.HasValue ? JsonConvert.SerializeObject(c.Value).GetHashCode() : 0,
            c => c.HasValue ? JsonConvert.DeserializeObject<KeyValuePair<string, string>>(JsonConvert.SerializeObject(c.Value)) : (KeyValuePair<string, string>?)null);

        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<User>(u => u.StudentID)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<User>(u => u.EmployeeID)
                .OnDelete(DeleteBehavior.SetNull);

            // For student
            entity.HasMany(u => u.EnrolledClasses)
                .WithMany(c => c.Students)
                .UsingEntity<StudentClass>("StudentClass",
                    l => l.HasOne<Class>().WithMany(c => c.StudentClasses).HasForeignKey(e => e.ClassID).OnDelete(DeleteBehavior.Cascade),
                    r => r.HasOne<User>().WithMany(c => c.StudentClasses).HasForeignKey(e => e.StudentID).OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.Property<int>("AbsencePercentage").HasDefaultValue(0);
                        j.HasKey("StudentID", "ClassID");
                    }
                );

            // For lecturer
            entity.HasMany(u => u.ManagedClasses)
                .WithOne(c => c.Lecturer)
                .HasForeignKey(c => c.LecturerID)
                .OnDelete(DeleteBehavior.NoAction);

            // For student
            entity.HasMany(u => u.Attendances)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SubstituteTeachings)
                .WithOne(s => s.SubstituteLecturer)
                .HasForeignKey(s => s.SubstituteLecturerID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.AssignedTeachings)
                .WithOne(s => s.OfficialLecturer)
                .HasForeignKey(s => s.OfficialLecturerID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasIndex(u => u.PhoneNumber).IsUnique();
        });

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
        });

        builder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");

            entity.HasMany(s => s.FingerprintTemplates)
                .WithOne(f => f.Student)
                .HasForeignKey(f => f.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => s.StudentCode).IsUnique();
        });

        builder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.HasMany(u => u.Modules)
                .WithOne(m => m.Employee)
                .HasForeignKey(m => m.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Module>(entity => {
            entity.ToTable("Module");

            entity.HasIndex(m => m.Key).IsUnique();
        });

        builder.Entity<Class>( entity => {
            entity.ToTable("Class");

            entity.HasMany(c => c.Schedules)
                .WithOne(s => s.Class)
                .HasForeignKey(s => s.ClassID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Subject)
                .WithMany(s => s.Classes)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Room)
                .WithMany(s => s.Classes)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Semester)
                .WithMany(s => s.Classes)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Schedule>( entity => {
            entity.ToTable("Schedule");

            entity.HasOne(s => s.Slot)
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.SlotID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(s => s.SubstituteTeaching)
                .WithOne(s => s.Schedule)
                .HasForeignKey<SubstituteTeaching>(s => s.ScheduleID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(s => s.Room)
                .WithMany(r => r.Schedules)
                .HasForeignKey(s => s.RoomID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(s => s.Attendances)
                .WithOne(a => a.Schedule)
                .HasForeignKey(a => a.ScheduleID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Slot>( entity => {
            entity.ToTable("Slot");

            entity.HasIndex(s => s.SlotNumber).IsUnique();
        });

        builder.Entity<Semester>(entity => {
            entity.ToTable("Semester");

            entity.HasIndex(s => s.SemesterCode).IsUnique();
        });

        builder.Entity<Subject>(entity => {
            entity.ToTable("Subject");

            entity.HasIndex(s => s.SubjectCode).IsUnique();
        });

        builder.Entity<Notification>().ToTable("Notification");

        builder.Entity<NotificationType>().ToTable("NotificationType");

        builder.Entity<FingerprintTemplate>().ToTable("FingerprintTemplate");

        builder.Entity<Room>().ToTable("Room");

        builder.Entity<SubstituteTeaching>().ToTable("SubstituteTeaching");

        builder.Entity<Attendance>().ToTable("Attendance");

        builder.Entity<PreparationTask>().ToTable("PreparationTask");

        builder.Entity<ModuleActivity>().ToTable("ModuleActivity");



        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();
        builder.Ignore<IdentityRoleClaim<Guid>>();
        builder.Ignore<IdentityRole<Guid>>();
        builder.Ignore<IdentityUserRole<Guid>>();
    }
}

public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("MsSQLConnection")!);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

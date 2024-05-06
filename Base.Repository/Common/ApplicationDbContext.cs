using Base.Repository.Entity;
using Base.Repository.Helper;
using Base.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Base.Repository.Common;

public interface IApplicationDbContext : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
}

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Class> Classes { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<AttendanceReport> AttendanceReports { get; set; } = null!;
    public DbSet<Campus> Campuses { get; set; } = null!;
    public DbSet<Curriculum> Curricula { get; set; } = null!;
    public DbSet<FingerprintTemplate> FingerprintTemplates { get; set; } = null!;
    public DbSet<FingerScanRecord> FingerScanRecords { get; set; } = null!;
    public DbSet<Module> Modules { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<ScheduleTable> ScheduleTables { get; set; } = null!;
    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<Slot> Slots { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;

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
            entity.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<IdentityUserRole<Guid>>();

                entity.HasMany(u => u.Classes)
                    .WithOne(c => c.Lecturer)
                    .HasForeignKey(c => c.LecturerID)
                    .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasIndex(u => u.PhoneNumber).IsUnique();
        });

        builder.Entity<Campus>(entity => {
            entity.HasMany(c => c.Rooms)
                .WithOne(r => r.Campus)
                .HasForeignKey(r => r.CampusID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Class>( entity => {
            entity.HasMany(c => c.Students)
                .WithMany(s => s.Classes)
                .UsingEntity("StudentClass", 
                    l => l.HasOne(typeof(Student)).WithMany().HasForeignKey("StudentID").OnDelete(DeleteBehavior.NoAction).HasPrincipalKey(nameof(Student.StudentID)),
                    r => r.HasOne(typeof(Class)).WithMany().HasForeignKey("ClassID").OnDelete(DeleteBehavior.NoAction).HasPrincipalKey(nameof(Class.ClassID)),
                    j => 
                    {
                        j.HasKey("StudentID", "ClassID");
                    }
                );
                
            entity.HasMany(c => c.ScheduleTables)
                .WithOne(s => s.Class)
                .HasForeignKey(s => s.ClassID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Course>( entity => {
            entity.HasMany(c => c.Classes)
                .WithOne(c => c.Course)
                .HasForeignKey(c => c.CourseID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Curriculum>( entity => {
            entity.HasMany(c => c.Students)
                .WithOne(s => s.Curriculum)
                .HasForeignKey(s => s.CurriculumID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(c => c.Subjects)
                .WithMany(s => s.Curriculums)
                .UsingEntity("CurriculumSubject",
                    l => l.HasOne(typeof(Subject)).WithMany().HasForeignKey("SubjectID").HasPrincipalKey(nameof(Subject.SubjectID)).OnDelete(DeleteBehavior.NoAction), 
                    r => r.HasOne(typeof(Curriculum)).WithMany().HasForeignKey("CurriculumID").HasPrincipalKey(nameof(Curriculum.CurriculumID)).OnDelete(DeleteBehavior.NoAction),
                    j => 
                    {
                        j.HasKey("SubjectID", "CurriculumID");
                    }
                );
        });

        builder.Entity<Module>( entity => {
            entity.HasOne(m => m.Room)
                .WithOne(r => r.Module)
                .HasForeignKey<Module>(m => m.RoomID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ScheduleTable>( entity => {
            entity.HasMany(s => s.AttendanceReports)
                .WithOne(a => a.ScheduleTable)
                .HasForeignKey(a => a.ScheduleID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(s => s.FingerScanRecords)
                .WithOne(a => a.ScheduleTable)
                .HasForeignKey(a => a.ScheduleID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Semester>( entity => {
            entity.HasMany(s => s.Courses)
                .WithOne(c => c.Semester)
                .HasForeignKey(c => c.SemesterID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Slot>( entity => {
            entity.HasMany(s => s.ScheduleTables)
                .WithOne(s => s.Slot)
                .HasForeignKey(s => s.SlotID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Student>( entity => {
            entity.HasMany(s => s.FingerprintTemplates)
                .WithOne(f => f.Student)
                .HasForeignKey(f => f.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(s => s.AttendanceReports)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Subject>( entity => {
            entity.HasMany(s => s.Courses)
                .WithOne(c => c.Subject)
                .HasForeignKey(c => c.SubjectID)
                .OnDelete(DeleteBehavior.NoAction);

        });

        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();
        builder.Ignore<IdentityRoleClaim<Guid>>();
    }
}

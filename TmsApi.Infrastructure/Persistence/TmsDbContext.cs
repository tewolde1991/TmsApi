using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identites;
namespace TmsApi.Infrastructure.Persistence;

public class TmsDbContext : IdentityDbContext<TmsUser>
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificate => Set<Certificate>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


    // We do not store EnrollmentRecord as DbSet – that's a domain event, not an entity.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // "Run Identity's model configuration first, then apply my application's configuration."
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);
        // global filter exclude archived enrollment by default
        modelBuilder.Entity<Enrollment>()
                    .HasQueryFilter(e => !e.IsArchived);


        Console.WriteLine("Configration loaded");
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            Console.WriteLine($"Entity:{entityType.ClrType.Name}");
        }
    }
}
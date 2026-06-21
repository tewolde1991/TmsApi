using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assesment => Set<Assessment>();
    public DbSet<Certificate> Certificate => Set<Certificate>();

    // We do not store EnrollmentRecord as DbSet – that's a domain event, not an entity.
}
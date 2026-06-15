// using Microsoft.EntityFrameworkCore;
// using TmsApi.Models;

// namespace TmsApi.Data;

// public class TMSDbContext : DbContext
// {
//     public TMSDbContext(DbContextOptions<TMSDbContext> options) : base(options) { }

//     public DbSet<Student> Students => Set<Student>();
//     public DbSet<Course> Courses => Set<Course>();
//     public DbSet<Grade> Grades => Set<Grade>();
//     // We do not store EnrollmentRecord as DbSet – that's a domain event, not an entity.
// }
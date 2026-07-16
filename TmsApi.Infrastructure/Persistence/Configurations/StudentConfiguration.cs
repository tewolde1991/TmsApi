using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);

        // Natural Key (Unique Constraint) - This is the "Session 2" requirement
        builder.HasIndex(s => s.RegistrationNumber)
               .IsUnique();

        // Property Constraints
        builder.Property(s => s.RegistrationNumber)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(s => s.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(s => s.GPA)
               .HasPrecision(3, 2); // e.g., 3.80

        // Relationships
        builder.HasMany(s => s.Enrollments)
               .WithOne(e => e.Student)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Student with active Enrollments
// shadow property: exists in model/DB NOT on student class

        builder.Property<DateTime>("LastUpdated");

       //  concurrency token mapped to postgrsql xmin
       builder.Property(s =>s.Version)
              .IsRowVersion();  // TELLS EF this is a concurrency token.[web:21][web:20]
    }
}
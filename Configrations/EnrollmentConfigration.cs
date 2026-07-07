
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

public class EnrollmentConfigrations : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e=>e.Year).IsRequired();
        builder.HasIndex(e =>new {e.StudentId, e.CourseId}).IsUnique();
         
        //  properties
        builder.Property(e =>e.Grade)
        .HasPrecision(4,2);

        

        builder.HasOne(e => e.Student)
        .WithMany(c=> c.Enrollments )
        .HasForeignKey(e =>e.StudentId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
        .WithMany(c=> c.Enrollments )
        .HasForeignKey(e =>e.CourseId)
        .OnDelete(DeleteBehavior.Restrict); // A course with existing enrollments cannot be deleted because 
// historical student grades must be preserved and never auto-deleted.

          // Prevents a student from enrolling in the same course twice
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
               .IsUnique();

        builder.Property(e => e.IsArchived)
                .HasDefaultValue(false);
    }
}
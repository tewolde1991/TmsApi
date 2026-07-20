using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        // Student → Enrollment: Restrict

        builder
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course → Enrollment: Restrict
        // because: Course deletes students' grade records not deleted—
        //          application code before enrollments deleted
        builder
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Grade — optional
        builder.Property(e => e.Grade)
            .HasColumnType("numeric(3,2)");

        // EnrolledAt — default now
        builder.Property(e => e.EnrolledAt)
            .HasDefaultValueSql("now()");
        builder.HasQueryFilter(e => !e.IsArchived);

        builder.Property(e => e.IsArchived)
            .HasDefaultValue(false);

        builder.Property(e => e.EnrolledAt)
            .HasDefaultValueSql("now()");
    }
}
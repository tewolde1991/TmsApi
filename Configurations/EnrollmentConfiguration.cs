using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        // Student → Enrollment: Restrict
        // ምክንያት: Student ሲሰረዝ enrollment records ትርጉም የላቸውም — ጥፋቱ ትክክል ነው
        builder
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course → Enrollment: Restrict
        // ምክንያት: Course ሲሰረዝ students' grade records መጥፋት የለባቸውም —
        //          application code ቀድሞ enrollments ማስወገድ አለበት
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
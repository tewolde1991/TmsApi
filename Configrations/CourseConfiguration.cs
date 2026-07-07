
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

public class CourseConfigration :IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.Code)
        .IsUnique();

        // property constraints
        builder.Property(c => c.Code)
        .HasMaxLength(20)
        .IsRequired();

        builder.Property(c =>c.Title)
        .HasMaxLength(100)
        .IsRequired();

        // Relashiship
        builder.HasMany( c => c.Enrollments)
        .WithOne(e =>e.Course)
        .HasForeignKey(e =>e.CourseId)
        .OnDelete(DeleteBehavior.Restrict);
    }

}
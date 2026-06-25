using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.GPA)
            .HasColumnType("numeric(3,2)");

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property<DateTime>("LastUpdated")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        // RowVersion — optimistic concurrency
        builder.Property(s => s.RowVersion)
            .IsRowVersion()
            .IsRequired();
        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.Property(s => s.IsDeleted)

                .HasDefaultValue(false);
    }
}
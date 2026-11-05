using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    private const string TableName = "Attendances";

    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Attendance> builder)
    {
        builder.Property(a => a.MemberId)
            .IsRequired()
            .HasColumnName(nameof(Attendance.MemberId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(a => a.CheckIn)
            .IsRequired()
            .HasColumnName(nameof(Attendance.CheckIn));

        builder.Property(a => a.CheckOut)
            .HasColumnName(nameof(Attendance.CheckOut));

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Attendance.CreatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasIndex(a => a.MemberId)
            .HasDatabaseName($"IX_{TableName}_{nameof(Attendance.MemberId)}");

        builder.HasIndex(a => a.CheckIn)
            .HasDatabaseName($"IX_{TableName}_{nameof(Attendance.CheckIn)}");

        builder.HasIndex(a => a.MemberId)
            .IsUnique()
            .HasFilter($"[{nameof(Attendance.CheckOut)}] IS NULL")
            .HasDatabaseName($"UX_{TableName}_{nameof(Attendance.MemberId)}_{nameof(Attendance.CheckOut)}_Open");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Attendance.CheckOut)}_{nameof(Attendance.CheckIn)}",
                $"\"{nameof(Attendance.CheckOut)}\" IS NULL OR \"{nameof(Attendance.CheckOut)}\" > \"{nameof(Attendance.CheckIn)}\"");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasOne(a => a.Member)
            .WithMany(m => m.Attendances)
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

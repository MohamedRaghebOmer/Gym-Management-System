using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    private const string TableName = "Members";

    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Member> builder)
    {
        builder.Property(m => m.PersonId)
            .IsRequired()
            .HasColumnName(nameof(Member.PersonId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(m => m.TrainerId)
            .HasColumnName(nameof(Member.TrainerId))
            .HasConversion(
                value => value == null ? null : value.Value,
                (int? value) => value == null ? null : Id.FromDatabase(value.Value));

        builder.Property(m => m.EmergencyContactName)
            .HasMaxLength(Member.Constants.EmergencyContactNameMaxLength)
            .HasColumnName(nameof(Member.EmergencyContactName));

        builder.Property(m => m.EmergencyContactPhone)
            .HasMaxLength(Member.Constants.EmergencyContactPhoneMaxLength)
            .HasColumnName(nameof(Member.EmergencyContactPhone));

        builder.Property(m => m.JoinDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName(nameof(Member.JoinDate));

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName(nameof(Member.IsActive));

        builder.Property(m => m.Notes)
            .HasMaxLength(Member.Constants.NotesMaxLength)
            .HasColumnName(nameof(Member.Notes));

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Member.CreatedAt));

        builder.Property(m => m.UpdatedAt)
            .HasColumnName(nameof(Member.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Member> builder)
    {
        builder.HasIndex(m => m.PersonId)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(Member.PersonId)}");

        builder.HasIndex(m => m.TrainerId)
            .IsUnique()
            .HasFilter($"[{nameof(Member.TrainerId)}] IS NOT NULL")
            .HasDatabaseName($"UX_{TableName}_{nameof(Member.TrainerId)}");

        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName($"IX_{TableName}_{nameof(Member.IsActive)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Member.EmergencyContactName)}",
                $"\"{nameof(Member.EmergencyContactName)}\" IS NULL OR length(\"{nameof(Member.EmergencyContactName)}\") <= {Member.Constants.EmergencyContactNameMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Member.EmergencyContactPhone)}",
                $"\"{nameof(Member.EmergencyContactPhone)}\" IS NULL OR length(\"{nameof(Member.EmergencyContactPhone)}\") <= {Member.Constants.EmergencyContactPhoneMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Member.Notes)}",
                $"\"{nameof(Member.Notes)}\" IS NULL OR length(\"{nameof(Member.Notes)}\") <= {Member.Constants.NotesMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<Member> builder)
    {
        builder.HasOne(m => m.Person)
            .WithOne(p => p.Member)
            .HasForeignKey<Member>(m => m.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Trainer)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

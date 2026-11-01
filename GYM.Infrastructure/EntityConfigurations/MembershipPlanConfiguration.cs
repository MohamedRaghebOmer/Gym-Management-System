using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    private const string TableName = "MembershipPlans";

    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.Property(mp => mp.GymId)
            .IsRequired()
            .HasColumnName(nameof(MembershipPlan.GymId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(mp => mp.Name)
            .IsRequired()
            .HasMaxLength(MembershipPlan.Constants.NameMaxLength)
            .HasColumnName(nameof(MembershipPlan.Name));

        builder.Property(mp => mp.DurationInDays)
            .IsRequired()
            .HasColumnName(nameof(MembershipPlan.DurationInDays));

        builder.Property(mp => mp.Price)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasColumnName(nameof(MembershipPlan.Price))
            .HasConversion(
                value => value.Value,
                value => Money.FromDatabase(value));

        builder.Property(mp => mp.Description)
            .HasMaxLength(MembershipPlan.Constants.DescriptionMaxLength)
            .HasColumnName(nameof(MembershipPlan.Description));

        builder.Property(mp => mp.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName(nameof(MembershipPlan.IsActive));

        builder.Property(mp => mp.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(MembershipPlan.CreatedAt));

        builder.Property(mp => mp.UpdatedAt)
            .HasColumnName(nameof(MembershipPlan.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.HasIndex(mp => mp.GymId)
            .HasDatabaseName($"IX_{TableName}_{nameof(MembershipPlan.GymId)}");

        builder.HasIndex(mp => mp.Name)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(MembershipPlan.Name)}");

        builder.HasIndex(mp => mp.IsActive)
            .HasDatabaseName($"IX_{TableName}_{nameof(MembershipPlan.IsActive)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(MembershipPlan.Name)}",
                $"length(\"{nameof(MembershipPlan.Name)}\") > 0 AND length(\"{nameof(MembershipPlan.Name)}\") <= {MembershipPlan.Constants.NameMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(MembershipPlan.DurationInDays)}",
                $"\"{nameof(MembershipPlan.DurationInDays)}\" > 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(MembershipPlan.Price)}",
                $"\"{nameof(MembershipPlan.Price)}\" > 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(MembershipPlan.Description)}",
                $"\"{nameof(MembershipPlan.Description)}\" IS NULL OR length(\"{nameof(MembershipPlan.Description)}\") <= {MembershipPlan.Constants.DescriptionMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.HasOne(mp => mp.Gym)
            .WithMany(g => g.MembershipPlans)
            .HasForeignKey(mp => mp.GymId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

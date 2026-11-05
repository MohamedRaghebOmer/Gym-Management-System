using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    private const string TableName = "Subscriptions";

    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(s => s.MemberId)
            .IsRequired()
            .HasColumnName(nameof(Subscription.MemberId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(s => s.MembershipPlanId)
            .IsRequired()
            .HasColumnName(nameof(Subscription.MembershipPlanId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(s => s.StartDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName(nameof(Subscription.StartDate));

        builder.Property(s => s.EndDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName(nameof(Subscription.EndDate));

        builder.Property(s => s.Price)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasColumnName(nameof(Subscription.Price))
            .HasConversion(
                value => value.Value,
                value => Money.FromDatabase(value));

        builder.Property(s => s.Status)
            .IsRequired()
            .HasColumnName(nameof(Subscription.Status))
            .HasConversion(
                value => (byte)value,
                value => (SubscriptionStatus)value);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Subscription.CreatedAt));

        builder.Property(s => s.UpdatedAt)
            .HasColumnName(nameof(Subscription.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasIndex(s => s.MemberId)
            .HasDatabaseName($"IX_{TableName}_{nameof(Subscription.MemberId)}");

        builder.HasIndex(s => s.MembershipPlanId)
            .HasDatabaseName($"IX_{TableName}_{nameof(Subscription.MembershipPlanId)}");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName($"IX_{TableName}_{nameof(Subscription.Status)}");

        builder.HasIndex(s => new { s.MemberId, s.Status, s.StartDate, s.EndDate })
            .HasDatabaseName($"IX_{TableName}_{nameof(Subscription.MemberId)}_{nameof(Subscription.Status)}_{nameof(Subscription.StartDate)}_{nameof(Subscription.EndDate)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Subscription.EndDate)}_{nameof(Subscription.StartDate)}",
                $"\"{nameof(Subscription.EndDate)}\" > \"{nameof(Subscription.StartDate)}\"");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Subscription.Price)}",
                $"\"{nameof(Subscription.Price)}\" > 0");

            var validStatusValues = string.Join(
                ", ",
                Enum.GetValues<SubscriptionStatus>()
                    .Select(value => (byte)value));

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Subscription.Status)}",
                $"[{nameof(Subscription.Status)}] IN ({validStatusValues})");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasOne(s => s.Member)
            .WithMany(m => m.Subscriptions)
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.MembershipPlan)
            .WithMany(mp => mp.Subscriptions)
            .HasForeignKey(s => s.MembershipPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

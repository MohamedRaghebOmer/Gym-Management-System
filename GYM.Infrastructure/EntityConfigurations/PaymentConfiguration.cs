using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    private const string TableName = "Payments";

    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.SubscriptionId)
            .IsRequired()
            .HasColumnName(nameof(Payment.SubscriptionId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasColumnName(nameof(Payment.Amount))
            .HasConversion(
                value => value.Value,
                value => Money.FromDatabase(value));

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasColumnName(nameof(Payment.PaymentMethod))
            .HasConversion(
                value => (byte)value,
                value => (PaymentMethod)value);

        builder.Property(p => p.PaymentDate)
            .IsRequired()
            .HasColumnName(nameof(Payment.PaymentDate));

        builder.Property(p => p.Notes)
            .HasMaxLength(Payment.Constants.NotesMaxLength)
            .HasColumnName(nameof(Payment.Notes));

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Payment.CreatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Payment> builder)
    {
        builder.HasIndex(p => p.SubscriptionId)
            .HasDatabaseName($"IX_{TableName}_{nameof(Payment.SubscriptionId)}");

        builder.HasIndex(p => p.PaymentDate)
            .HasDatabaseName($"IX_{TableName}_{nameof(Payment.PaymentDate)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Payment.Amount)}",
                $"\"{nameof(Payment.Amount)}\" > 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Payment.Notes)}",
                $"\"{nameof(Payment.Notes)}\" IS NULL OR length(\"{nameof(Payment.Notes)}\") <= {Payment.Constants.NotesMaxLength}");

            var validPaymentMethods = string.Join(
                ", ",
                Enum.GetValues<PaymentMethod>()
                    .Select(value => (byte)value));

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Payment.PaymentMethod)}",
                $"[{nameof(Payment.PaymentMethod)}] IN ({validPaymentMethods})");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<Payment> builder)
    {
        builder.HasOne(p => p.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

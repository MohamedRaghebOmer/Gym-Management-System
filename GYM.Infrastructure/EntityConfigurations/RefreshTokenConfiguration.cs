using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    private const string TableName = "RefreshTokens";

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasColumnName(nameof(RefreshToken.UserId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(RefreshToken.Constants.TokenMaxLength)
            .HasColumnName(nameof(RefreshToken.Token));

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired()
            .HasColumnName(nameof(RefreshToken.ExpiresAt));

        builder.Property(rt => rt.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(RefreshToken.CreatedAt));

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName(nameof(RefreshToken.RevokedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(RefreshToken.Token)}");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName($"IX_{TableName}_{nameof(RefreshToken.UserId)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(RefreshToken.Token)}",
                $"length(\"{nameof(RefreshToken.Token)}\") > 0 AND length(\"{nameof(RefreshToken.Token)}\") <= {RefreshToken.Constants.TokenMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const string TableName = "Users";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(
        EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.PersonId)
            .IsRequired()
            .HasColumnName(nameof(User.PersonId))
            .HasConversion(
                v => v.Value,
                v => Id.FromDatabase(v));

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(User.Constants.UsernameMaxLength)
            .HasColumnName(nameof(User.Username));

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(User.Constants.PasswordHashMaxLength)
            .HasColumnName(nameof(User.PasswordHash));

        builder.Property(u => u.RoleId)
            .IsRequired()
            .HasColumnName(nameof(User.RoleId))
            .HasConversion(
                v => v.Value,
                v => Id.FromDatabase(v));

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasColumnName(nameof(User.IsActive))
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(User.CreatedAt));

        builder.Property(u => u.UpdatedAt)
            .HasColumnName(nameof(User.UpdatedAt));
    }

    private static void ConfigureIndexes(
        EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.PersonId)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(User.PersonId)}");

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(User.Username)}");

        builder.HasIndex(u => u.RoleId)
            .HasDatabaseName($"IX_{TableName}_{nameof(User.RoleId)}");
    }

    private static void ConfigureConstraints(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(User.Username)}",
                $"length(\"{nameof(User.Username)}\") >= {User.Constants.UsernameMinLength} AND length(\"{nameof(User.Username)}\") <= {User.Constants.UsernameMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(User.PasswordHash)}",
                $"length(\"{nameof(User.PasswordHash)}\") <= {User.Constants.PasswordHashMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(
        EntityTypeBuilder<User> builder)
    {
        builder.HasOne(u => u.Person)
            .WithOne()
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using GYM.Domain.Entities;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private const string TableName = "Roles";

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(Role.Constants.NameMaxLength)
            .HasColumnName(nameof(Role.Name));

        builder.Property(r => r.Description)
            .HasMaxLength(Role.Constants.DescriptionMaxLength)
            .HasColumnName(nameof(Role.Description));

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Role.CreatedAt));

        builder.Property(r => r.UpdatedAt)
            .HasColumnName(nameof(Role.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(Role.Name)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Role.Name)}",
                $"length(\"{nameof(Role.Name)}\") >= {Role.Constants.NameMinLength} AND length(\"{nameof(Role.Name)}\") <= {Role.Constants.NameMaxLength}");
            
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Role.Description)}",
                $"\"{nameof(Role.Description)}\" IS NULL OR length(\"{nameof(Role.Description)}\") <= {Role.Constants.DescriptionMaxLength}");
        });
    }
}

using GYM.Domain.Entities;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    private const string TableName = "Gyms";

    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Gym> builder)
    {
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(Gym.Constants.NameMaxLength)
            .HasColumnName(nameof(Gym.Name));

        builder.Property(g => g.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Gym.CreatedAt));

        builder.Property(g => g.UpdatedAt)
            .HasColumnName(nameof(Gym.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Gym> builder)
    {
        builder.HasIndex(g => g.Name)
            .HasDatabaseName($"IX_{TableName}_{nameof(Gym.Name)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Gym> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Gym.Name)}",
                $"length(\"{nameof(Gym.Name)}\") >= {Gym.Constants.NameMinLength} AND length(\"{nameof(Gym.Name)}\") <= {Gym.Constants.NameMaxLength}");
        });
    }
}

using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    private const string TableName = "Exercises";

    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Exercise> builder)
    {
        builder.Property(e => e.GymId)
            .IsRequired()
            .HasColumnName(nameof(Exercise.GymId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(Exercise.Constants.NameMaxLength)
            .HasColumnName(nameof(Exercise.Name));

        builder.Property(e => e.MuscleGroup)
            .IsRequired()
            .HasMaxLength(Exercise.Constants.MuscleGroupMaxLength)
            .HasColumnName(nameof(Exercise.MuscleGroup));

        builder.Property(e => e.Description)
            .HasMaxLength(Exercise.Constants.DescriptionMaxLength)
            .HasColumnName(nameof(Exercise.Description));

        builder.Property(e => e.Instructions)
            .HasMaxLength(Exercise.Constants.InstructionsMaxLength)
            .HasColumnName(nameof(Exercise.Instructions));

        builder.Property(e => e.VideoUrl)
            .HasMaxLength(Exercise.Constants.VideoUrlMaxLength)
            .HasColumnName(nameof(Exercise.VideoUrl));

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName(nameof(Exercise.IsActive));

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Exercise.CreatedAt));

        builder.Property(e => e.UpdatedAt)
            .HasColumnName(nameof(Exercise.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasIndex(e => e.GymId)
            .HasDatabaseName($"IX_{TableName}_{nameof(Exercise.GymId)}");

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(Exercise.Name)}");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName($"IX_{TableName}_{nameof(Exercise.IsActive)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Exercise.Name)}",
                $"length(\"{nameof(Exercise.Name)}\") > 0 AND length(\"{nameof(Exercise.Name)}\") <= {Exercise.Constants.NameMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Exercise.MuscleGroup)}",
                $"length(\"{nameof(Exercise.MuscleGroup)}\") > 0 AND length(\"{nameof(Exercise.MuscleGroup)}\") <= {Exercise.Constants.MuscleGroupMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Exercise.Description)}",
                $"\"{nameof(Exercise.Description)}\" IS NULL OR length(\"{nameof(Exercise.Description)}\") <= {Exercise.Constants.DescriptionMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Exercise.Instructions)}",
                $"\"{nameof(Exercise.Instructions)}\" IS NULL OR length(\"{nameof(Exercise.Instructions)}\") <= {Exercise.Constants.InstructionsMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Exercise.VideoUrl)}",
                $"\"{nameof(Exercise.VideoUrl)}\" IS NULL OR length(\"{nameof(Exercise.VideoUrl)}\") <= {Exercise.Constants.VideoUrlMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasOne(e => e.Gym)
            .WithMany(g => g.Exercises)
            .HasForeignKey(e => e.GymId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

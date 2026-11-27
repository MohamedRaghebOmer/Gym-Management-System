using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    private const string TableName = "WorkoutPlanExercises";

    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.Property(wpe => wpe.WorkoutPlanId)
            .IsRequired()
            .HasColumnName(nameof(WorkoutPlanExercise.WorkoutPlanId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(wpe => wpe.ExerciseId)
            .IsRequired()
            .HasColumnName(nameof(WorkoutPlanExercise.ExerciseId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(wpe => wpe.Sets)
            .IsRequired()
            .HasColumnName(nameof(WorkoutPlanExercise.Sets));

        builder.Property(wpe => wpe.Reps)
            .HasColumnName(nameof(WorkoutPlanExercise.Reps));

        builder.Property(wpe => wpe.Weight)
            .HasPrecision(6, 2)
            .HasColumnName(nameof(WorkoutPlanExercise.Weight));

        builder.Property(wpe => wpe.RestSeconds)
            .HasColumnName(nameof(WorkoutPlanExercise.RestSeconds));

        builder.Property(wpe => wpe.DisplayOrder)
            .IsRequired()
            .HasColumnName(nameof(WorkoutPlanExercise.DisplayOrder));

        builder.Property(wpe => wpe.Notes)
            .HasMaxLength(WorkoutPlanExercise.Constants.NotesMaxLength)
            .HasColumnName(nameof(WorkoutPlanExercise.Notes));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasIndex(wpe => new { wpe.WorkoutPlanId, wpe.ExerciseId })
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(WorkoutPlanExercise.WorkoutPlanId)}_{nameof(WorkoutPlanExercise.ExerciseId)}");

        builder.HasIndex(wpe => wpe.ExerciseId)
            .HasDatabaseName($"IX_{TableName}_{nameof(WorkoutPlanExercise.ExerciseId)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(WorkoutPlanExercise.Sets)}",
                $"\"{nameof(WorkoutPlanExercise.Sets)}\" > 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(WorkoutPlanExercise.Reps)}",
                $"\"{nameof(WorkoutPlanExercise.Reps)}\" IS NULL OR \"{nameof(WorkoutPlanExercise.Reps)}\" > 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(WorkoutPlanExercise.Weight)}",
                $"\"{nameof(WorkoutPlanExercise.Weight)}\" IS NULL OR \"{nameof(WorkoutPlanExercise.Weight)}\" >= 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(WorkoutPlanExercise.RestSeconds)}",
                $"\"{nameof(WorkoutPlanExercise.RestSeconds)}\" IS NULL OR \"{nameof(WorkoutPlanExercise.RestSeconds)}\" >= 0");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(WorkoutPlanExercise.Notes)}",
                $"\"{nameof(WorkoutPlanExercise.Notes)}\" IS NULL OR length(\"{nameof(WorkoutPlanExercise.Notes)}\") <= {WorkoutPlanExercise.Constants.NotesMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasOne(wpe => wpe.WorkoutPlan)
            .WithMany(wp => wp.WorkoutPlanExercises)
            .HasForeignKey(wpe => wpe.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wpe => wpe.Exercise)
            .WithMany(e => e.WorkoutPlanExercises)
            .HasForeignKey(wpe => wpe.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

public sealed class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    private const string TableName = "WorkoutPlans";

    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.Property(p => p.MemberId)
            .IsRequired()
            .HasColumnName(nameof(WorkoutPlan.MemberId))
            .HasConversion(
                id => id.Value,
                value => Id.FromDatabase(value));

        builder.Property(p => p.TrainerId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => Id.FromDatabase(value));

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(WorkoutPlan.Constants.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(WorkoutPlan.Constants.DescriptionMaxLength);

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate);

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasIndex(p => p.MemberId)
            .HasDatabaseName($"IX_{TableName}_{nameof(WorkoutPlan.MemberId)}");

        builder.HasIndex(p => p.TrainerId)
            .HasDatabaseName($"IX_{TableName}_{nameof(WorkoutPlan.TrainerId)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.ToTable(table =>
        {
            // Check constraint to ensure that EndDate is either null or greater than StartDate
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(WorkoutPlan.EndDate)}_{nameof(WorkoutPlan.StartDate)}",
                $"\"{nameof(WorkoutPlan.EndDate)}\" IS NULL OR \"{nameof(WorkoutPlan.EndDate)}\" > \"{nameof(WorkoutPlan.StartDate)}\"");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasOne(p => p.Member)
            .WithMany(m => m.WorkoutPlans)
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Trainer)
            .WithMany(t => t.WorkoutPlans)
            .HasForeignKey(p => p.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    private const string TableName = "Trainers";

    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
        ConfigureForeignKeys(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Trainer> builder)
    {
        builder.Property(t => t.PersonId)
            .IsRequired()
            .HasColumnName(nameof(Trainer.PersonId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(t => t.Specialization)
            .HasMaxLength(Trainer.Constants.SpecializationMaxLength)
            .HasColumnName(nameof(Trainer.Specialization));

        builder.Property(t => t.HireDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName(nameof(Trainer.HireDate));

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName(nameof(Trainer.IsActive));

        builder.Property(t => t.Notes)
            .HasMaxLength(Trainer.Constants.NotesMaxLength)
            .HasColumnName(nameof(Trainer.Notes));

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Trainer.CreatedAt));

        builder.Property(t => t.UpdatedAt)
            .HasColumnName(nameof(Trainer.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Trainer> builder)
    {
        builder.HasIndex(t => t.PersonId)
            .IsUnique()
            .HasDatabaseName($"UX_{TableName}_{nameof(Trainer.PersonId)}");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName($"IX_{TableName}_{nameof(Trainer.IsActive)}");
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Trainer.Specialization)}",
                $"\"{nameof(Trainer.Specialization)}\" IS NULL OR length(\"{nameof(Trainer.Specialization)}\") <= {Trainer.Constants.SpecializationMaxLength}");

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Trainer.Notes)}",
                $"\"{nameof(Trainer.Notes)}\" IS NULL OR length(\"{nameof(Trainer.Notes)}\") <= {Trainer.Constants.NotesMaxLength}");
        });
    }

    private static void ConfigureForeignKeys(EntityTypeBuilder<Trainer> builder)
    {
        builder.HasOne(t => t.Person)
            .WithOne(p => p.Trainer)
            .HasForeignKey<Trainer>(t => t.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

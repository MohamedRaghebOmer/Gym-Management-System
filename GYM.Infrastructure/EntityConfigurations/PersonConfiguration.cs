using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.EntityConfigurations;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    /// <summary>
    /// TableName is written like that likely specialy in PersonConfiguration class because the domain model name is completly different from the table name in the database. The domain model is called Person, but the table name is People. This is a common practice in software development to have different names for domain models and database tables to better reflect their purpose and context.
    /// But if the domain model name will be only different from the table name in only 's' letter, then use BaseEntityConfiguration class to configure the table name automatically. But if the domain model name is completly different from the table name, then use this approach to configure the table name manually.
    /// </summary>
    private const string TableName = "People";

    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ConfigurePrimaryKey();
        builder.ToTable(TableName);

        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureCheckConstraints(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.GymId)
            .IsRequired()
            .HasColumnName(nameof(Person.GymId))
            .HasConversion(
                value => value.Value,
                value => Id.FromDatabase(value));

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(Person.Constants.FullNameMaxLength)
            .HasColumnName(nameof(Person.FullName));

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(Person.Constants.PhoneMaxLength)
            .HasColumnName(nameof(Person.Phone))
            .HasConversion(
                value => value.Value,
                value => Phone.FromDatabase(value));

        builder.Property(p => p.Email)
            .HasMaxLength(Person.Constants.EmailMaxLength)
            .HasColumnName(nameof(Person.Email))
            .HasConversion(
                value => value!.Value,
                value => Email.FromDatabase(value));

        builder.Property(p => p.DateOfBirth)
            .HasColumnName(nameof(Person.DateOfBirth))
            .HasConversion(
                value => value!.Value,
                value => DateOfBirth.FromDatabase(value));

        builder.Property(p => p.Gender)
            .HasColumnName(nameof(Person.Gender))
            .IsRequired()
            .HasConversion(
                value => (byte)value,
                value => (Gender)value);

        builder.Property(p => p.Address)
            .HasMaxLength(Person.Constants.AddressMaxLength)
            .HasColumnName(nameof(Person.Address));

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName(nameof(Person.CreatedAt));

        builder.Property(p => p.UpdatedAt)
            .HasColumnName(nameof(Person.UpdatedAt));
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Person> builder)
    {
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasFilter($"[{nameof(Person.Email)}] IS NOT NULL")
            .HasDatabaseName($"UX_{TableName}_{nameof(Person.Email)}"); // Prefix "UX_" for unique index

        builder.HasIndex(p => p.GymId)
            .HasDatabaseName($"IX_{TableName}_{nameof(Person.GymId)}"); // Prefix "IX_" for non-unique index
    }

    private static void ConfigureCheckConstraints(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable(table =>
        {
            // Check constraint for FullName length
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Person.FullName)}",
                $"length(\"{nameof(Person.FullName)}\") >= {Person.Constants.FullNameMinLength} AND length(\"{nameof(Person.FullName)}\") <= {Person.Constants.FullNameMaxLength}");

            // Check constraint for Phone length
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Person.Phone)}",
                $"length(\"{nameof(Person.Phone)}\") >= {Person.Constants.PhoneMinLength} AND length(\"{nameof(Person.Phone)}\") <= {Person.Constants.PhoneMaxLength}");

            // Check the Gender
            var validGenderValues = string.Join(
                ", ",
                Enum.GetValues<Gender>()
                    .Select(value => (byte)value));

            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Person.Gender)}",
                $"[{nameof(Person.Gender)}] IN ({validGenderValues})");

            // Check constraint for Email length
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Person.Email)}",
                $"[{nameof(Person.Email)}] IS NULL OR length({nameof(Person.Email)}) <= {Person.Constants.EmailMaxLength}");

            // Check constraint for Address length
            table.HasCheckConstraint(
                $"CK_{TableName}_{nameof(Person.Address)}",
                $"[{nameof(Person.Address)}] IS NULL OR length({nameof(Person.Address)}) <= {Person.Constants.AddressMaxLength}");
        });
    }
}
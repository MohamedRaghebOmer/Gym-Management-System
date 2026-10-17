using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Role : Entity
{
    public static class Constants
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 250;
    }

    private Role(Id id, string name, string? description)
        :base(id)
    {
        Name = name;
        Description = description;
    }

    private Role(string name, string? description)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    private Role() { } // For EF Core

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public static Result<Role> Create(string name, string? description)
    {
        name = name.Trim();
        description = description?.Trim();

        var result = Validate(name, description);
        if (result.IsFailure)
            return Result.Failure<Role>(result.Error);

        return new Role(name, description);
    }

    public Result Update(string name, string? description)
    {
        name = name.Trim();
        description = description?.Trim();

        var result = Validate(name, description);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(string name, string? description)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(DomainErrors.Entities.Role.Name.Empty);

        if (name.Length < Constants.NameMinLength)
            errors.Add(DomainErrors.Entities.Role.Name.TooShort);

        if (name.Length > Constants.NameMaxLength)
            errors.Add(DomainErrors.Entities.Role.Name.TooLong);

        if (!string.IsNullOrWhiteSpace(description) && description!.Length > Constants.DescriptionMaxLength)
            errors.Add(DomainErrors.Entities.Role.Description.TooLong);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }


    // Seed data for the Role entity
    public static Role[] SeedData()
    {
        return
        [
            new Role(Id.FromDatabase(1),
                "Admin",
                "Administrator role with full access"),

            new Role(Id.FromDatabase(2),
                "Trainer",
                "Trainer role with training responsibilities")
        ];
    }
}

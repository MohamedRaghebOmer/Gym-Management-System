using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;

namespace GYM.Domain.Entities;

public sealed class Gym : Entity
{
    public static class Constants
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 150;
    }

    private Gym(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    private Gym() { } // For EF Core

    public string Name { get; private set; } = null!;
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public static Result<Gym> Create(string name)
    {
        name = name.Trim();

        var result = Validate(name);
        if (result.IsFailure)
            return Result.Failure<Gym>(result.Error);

        return new Gym(name);
    }

    public Result Update(string name)
    {
        name = name.Trim();

        var result = Validate(name);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Name = name;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(string name)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(DomainErrors.Entities.Gym.Name.Empty);

        if (name.Length < Constants.NameMinLength)
            errors.Add(DomainErrors.Entities.Gym.Name.TooShort);

        if (name.Length > Constants.NameMaxLength)
            errors.Add(DomainErrors.Entities.Gym.Name.TooLong);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

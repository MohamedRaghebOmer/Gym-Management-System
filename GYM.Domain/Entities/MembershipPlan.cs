using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class MembershipPlan : Entity
{
    public static class Constants
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
    }

    private MembershipPlan(
        Id gymId,
        string name,
        int durationInDays,
        Money price,
        string? description)
    {
        GymId = gymId;
        Name = name;
        DurationInDays = durationInDays;
        Price = price;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    private MembershipPlan() { } // For EF Core

    public Id GymId { get; private set; } = default!;
    public string Name { get; private set; } = null!;
    public int DurationInDays { get; private set; }
    public Money Price { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public Gym? Gym { get; private set; }

    public static Result<MembershipPlan> Create(
        Id gymId,
        string name,
        int durationInDays,
        Money price,
        string? description)
    {
        name = name.Trim();
        description = description?.Trim();

        var result = Validate(name, durationInDays, description);
        if (result.IsFailure)
            return Result.Failure<MembershipPlan>(result.Error);

        return new MembershipPlan(gymId, name, durationInDays, price, description);
    }

    public Result Update(
        string name,
        int durationInDays,
        Money price,
        string? description,
        bool isActive)
    {
        name = name.Trim();
        description = description?.Trim();

        var result = Validate(name, durationInDays, description);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Name = name;
        DurationInDays = durationInDays;
        Price = price;
        Description = description;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        string name,
        int durationInDays,
        string? description)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(DomainErrors.Entities.MembershipPlan.Name.Empty);

        if (name.Length > Constants.NameMaxLength)
            errors.Add(DomainErrors.Entities.MembershipPlan.Name.TooLong);

        if (durationInDays <= 0)
            errors.Add(DomainErrors.Entities.MembershipPlan.DurationInDays.LessThanOrEqualToZero);

        if (!string.IsNullOrWhiteSpace(description) 
            && description.Length > Constants.DescriptionMaxLength)
            errors.Add(DomainErrors.Entities.MembershipPlan.Description.TooLong);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

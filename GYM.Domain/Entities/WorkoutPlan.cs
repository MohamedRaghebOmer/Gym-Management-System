using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class WorkoutPlan : Entity
{
    public static class Constants
    {
        public const int NameMaxLength = 150;
        public const int DescriptionMaxLength = 1000;
    }

    private WorkoutPlan(
        Id memberId,
        Id trainerId,
        string name,
        string? description,
        DateOnly startDate,
        DateOnly? endDate)
    {
        MemberId = memberId;
        TrainerId = trainerId;
        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    private WorkoutPlan() { } // For EF Core

    public Id MemberId { get; private set; } = default!;
    public Id TrainerId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public Member? Member { get; private set; }
    public Trainer? Trainer { get; private set; }

    public static Result<WorkoutPlan> Create(
        Id memberId,
        Id trainerId,
        string name,
        string? description,
        DateOnly startDate,
        DateOnly? endDate)
    {
        name = name.Trim();
        description = description?.Trim();

        var result = Validate(name, description, startDate, endDate);
        if (result.IsFailure)
            return Result.Failure<WorkoutPlan>(result.Error);

        return new WorkoutPlan(
            memberId,
            trainerId,
            name,
            description,
            startDate,
            endDate);
    }

    public Result Update(
        string name,
        string? description,
        DateOnly startDate,
        DateOnly? endDate,
        bool isActive)
    {
        name = name.Trim();
        description = description?.Trim();

        var result = Validate(name, description, startDate, endDate);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        string name,
        string? description,
        DateOnly startDate,
        DateOnly? endDate)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(DomainErrors.Entities.WorkoutPlan.Name.Required);
        else if (name.Length > Constants.NameMaxLength)
            errors.Add(DomainErrors.Entities.WorkoutPlan.Name.TooLong);

        if (!string.IsNullOrWhiteSpace(description) &&
            description.Length > Constants.DescriptionMaxLength)
            errors.Add(DomainErrors.Entities.WorkoutPlan.Description.TooLong);

        if (startDate == default)
            errors.Add(DomainErrors.Entities.WorkoutPlan.StartDate.Required);

        if (endDate.HasValue && endDate.Value < startDate)
            errors.Add(DomainErrors.Entities.WorkoutPlan.EndDate.BeforeStartDate);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}
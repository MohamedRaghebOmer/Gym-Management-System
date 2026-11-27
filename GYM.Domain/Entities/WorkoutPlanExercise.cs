using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class WorkoutPlanExercise : Entity
{
    public static class Constants
    {
        public const int NotesMaxLength = 500;
    }

    private WorkoutPlanExercise(
        Id workoutPlanId,
        Id exerciseId,
        byte sets,
        byte? reps,
        decimal? weight,
        short? restSeconds,
        byte displayOrder,
        string? notes)
    {
        WorkoutPlanId = workoutPlanId;
        ExerciseId = exerciseId;
        Sets = sets;
        Reps = reps;
        Weight = weight;
        RestSeconds = restSeconds;
        DisplayOrder = displayOrder;
        Notes = notes;
    }

    private WorkoutPlanExercise() { } // For EF Core

    public Id WorkoutPlanId { get; private set; } = default!;
    public Id ExerciseId { get; private set; } = default!;
    public byte Sets { get; private set; }
    public byte? Reps { get; private set; }
    public decimal? Weight { get; private set; }
    public short? RestSeconds { get; private set; }
    public byte DisplayOrder { get; private set; }
    public string? Notes { get; private set; }

    public WorkoutPlan? WorkoutPlan { get; private set; }
    public Exercise? Exercise { get; private set; }

    public static Result<WorkoutPlanExercise> Create(
        Id workoutPlanId,
        Id exerciseId,
        byte sets,
        byte? reps,
        decimal? weight,
        short? restSeconds,
        byte displayOrder,
        string? notes)
    {
        notes = notes?.Trim();

        var result = Validate(sets, reps, weight, restSeconds, notes);
        if (result.IsFailure)
            return Result.Failure<WorkoutPlanExercise>(result.Error);

        return new WorkoutPlanExercise(
            workoutPlanId,
            exerciseId,
            sets,
            reps,
            weight,
            restSeconds,
            displayOrder,
            notes);
    }

    public Result Update(
        byte sets,
        byte? reps,
        decimal? weight,
        short? restSeconds,
        byte displayOrder,
        string? notes)
    {
        notes = notes?.Trim();

        var result = Validate(sets, reps, weight, restSeconds, notes);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Sets = sets;
        Reps = reps;
        Weight = weight;
        RestSeconds = restSeconds;
        DisplayOrder = displayOrder;
        Notes = notes;

        return Result.Success();
    }

    private static Result Validate(
        byte sets,
        byte? reps,
        decimal? weight,
        short? restSeconds,
        string? notes)
    {
        List<Error> errors = [];

        if (sets <= 0)
            errors.Add(DomainErrors.Entities.WorkoutPlanExercise.Sets.LessThanOrEqualToZero);

        if (reps.HasValue && reps.Value <= 0)
            errors.Add(DomainErrors.Entities.WorkoutPlanExercise.Reps.LessThanOrEqualToZero);

        if (weight.HasValue && weight.Value < 0)
            errors.Add(DomainErrors.Entities.WorkoutPlanExercise.Weight.Negative);

        if (restSeconds.HasValue && restSeconds.Value < 0)
            errors.Add(DomainErrors.Entities.WorkoutPlanExercise.RestSeconds.Negative);

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > Constants.NotesMaxLength)
            errors.Add(DomainErrors.Entities.WorkoutPlanExercise.Notes.TooLong);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Trainer : Entity
{
    public static class Constants
    {
        public const int SpecializationMaxLength = 150;
        public const int NotesMaxLength = 1000;
    }

    private Trainer(
        Id personId,
        string? specialization,
        DateTime hireDate,
        string? notes)
    {
        PersonId = personId;
        Specialization = specialization;
        HireDate = hireDate;
        IsActive = true;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    private Trainer() { } // For EF Core

    public Id PersonId { get; private set; } = default!;
    public string? Specialization { get; private set; }
    public DateTime HireDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public Person? Person { get; private set; }
    public ICollection<Member> Members { get; private set; } = [];

    public static Result<Trainer> Create(
        Id personId,
        string? specialization,
        DateTime hireDate,
        string? notes)
    {
        specialization = specialization?.Trim();
        notes = notes?.Trim();

        var result = Validate(specialization, notes, hireDate);
        if (result.IsFailure)
            return Result.Failure<Trainer>(result.Error);

        return new Trainer(personId, specialization, hireDate, notes);
    }

    public Result Update(
        string? specialization,
        bool isActive,
        string? notes)
    {
        specialization = specialization?.Trim();
        notes = notes?.Trim();

        var result = Validate(specialization, notes, null);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Specialization = specialization;
        IsActive = isActive;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        string? specialization,
        string? notes,
        DateTime? hireDate)
    {
        List<Error> errors = [];

        if (!string.IsNullOrWhiteSpace(specialization) && specialization.Length > Constants.SpecializationMaxLength)
            errors.Add(DomainErrors.Entities.Trainer.Specialization.TooLong);

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > Constants.NotesMaxLength)
            errors.Add(DomainErrors.Entities.Trainer.Notes.TooLong);

        if (hireDate.HasValue)
        {
            if (hireDate.Value == default)
                errors.Add(DomainErrors.Entities.Trainer.HireDate.Required);
            else if (hireDate.Value > DateTime.UtcNow)
                errors.Add(DomainErrors.Entities.Trainer.HireDate.InTheFuture);
        }

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

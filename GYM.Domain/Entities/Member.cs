using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Member : Entity
{
    public static class Constants
    {
        public const int EmergencyContactNameMaxLength = 150;
        public const int EmergencyContactPhoneMaxLength = 20;
        public const int NotesMaxLength = 1000;
    }

    private Member(
        Id personId,
        Id? trainerId,
        string? emergencyContactName,
        string? emergencyContactPhone,
        DateTime joinDate,
        string? notes)
    {
        PersonId = personId;
        TrainerId = trainerId;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        JoinDate = joinDate;
        IsActive = true;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    private Member() { } // For EF Core

    public Id PersonId { get; private set; } = default!;
    public Id? TrainerId { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public DateTime JoinDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public Person? Person { get; private set; }
    public Trainer? Trainer { get; private set; }

    public static Result<Member> Create(
        Id personId,
        Id? trainerId,
        string? emergencyContactName,
        string? emergencyContactPhone,
        DateTime joinDate,
        string? notes)
    {
        emergencyContactName = emergencyContactName?.Trim();
        emergencyContactPhone = emergencyContactPhone?.Trim();
        notes = notes?.Trim();

        var result = Validate(emergencyContactName, emergencyContactPhone, notes, joinDate);
        if (result.IsFailure)
            return Result.Failure<Member>(result.Error);

        return new Member(
            personId,
            trainerId,
            emergencyContactName,
            emergencyContactPhone,
            joinDate,
            notes);
    }

    public Result Update(
        Id? trainerId,
        string? emergencyContactName,
        string? emergencyContactPhone,
        bool isActive,
        string? notes)
    {
        emergencyContactName = emergencyContactName?.Trim();
        emergencyContactPhone = emergencyContactPhone?.Trim();
        notes = notes?.Trim();

        var result = Validate(emergencyContactName, emergencyContactPhone, notes, null);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        TrainerId = trainerId;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        IsActive = isActive;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        string? emergencyContactName,
        string? emergencyContactPhone,
        string? notes,
        DateTime? joinDate)
    {
        List<Error> errors = [];

        if (!string.IsNullOrWhiteSpace(emergencyContactName) && emergencyContactName.Length > Constants.EmergencyContactNameMaxLength)
            errors.Add(DomainErrors.Entities.Member.EmergencyContactName.TooLong);

        if (!string.IsNullOrWhiteSpace(emergencyContactPhone) && emergencyContactPhone.Length > Constants.EmergencyContactPhoneMaxLength)
            errors.Add(DomainErrors.Entities.Member.EmergencyContactPhone.TooLong);

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > Constants.NotesMaxLength)
            errors.Add(DomainErrors.Entities.Member.Notes.TooLong);

        if (joinDate.HasValue)
        {
            if (joinDate.Value == default)
                errors.Add(DomainErrors.Entities.Member.JoinDate.Required);
            else if (joinDate.Value.Date > DateTime.UtcNow.Date)
                errors.Add(DomainErrors.Entities.Member.JoinDate.InTheFuture);
        }

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

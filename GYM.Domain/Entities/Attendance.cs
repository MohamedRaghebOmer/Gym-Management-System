using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Attendance : Entity
{
    private Attendance(
        Id memberId,
        DateTime checkIn,
        DateTime? checkOut)
    {
        MemberId = memberId;
        CheckIn = checkIn;
        CheckOut = checkOut;
        CreatedAt = DateTime.UtcNow;
    }

    private Attendance() { } // For EF Core

    public Id MemberId { get; private set; } = default!;
    public DateTime CheckIn { get; private set; }
    public DateTime? CheckOut { get; private set; }
    public DateTime CreatedAt { get; private init; }

    public Member? Member { get; private set; }

    public static Result<Attendance> Create(
        Id memberId,
        DateTime checkIn,
        DateTime? checkOut)
    {
        var result = Validate(checkIn, checkOut);
        if (result.IsFailure)
            return Result.Failure<Attendance>(result.Error);

        return new Attendance(memberId, checkIn, checkOut);
    }

    public Result Update(
        DateTime checkIn,
        DateTime? checkOut)
    {
        var result = Validate(checkIn, checkOut);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        CheckIn = checkIn;
        CheckOut = checkOut;

        return Result.Success();
    }

    private static Result Validate(
        DateTime checkIn,
        DateTime? checkOut)
    {
        List<Error> errors = [];

        if (checkIn == default)
            errors.Add(DomainErrors.Entities.Attendance.CheckIn.Required);

        if (checkOut.HasValue && checkIn != default && checkOut.Value <= checkIn)
            errors.Add(DomainErrors.Entities.Attendance.CheckOut.MustBeGreaterThanCheckIn);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

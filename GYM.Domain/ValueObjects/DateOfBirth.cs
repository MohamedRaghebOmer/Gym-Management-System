using GYM.Domain.Errors;
using GYM.Domain.Shared;

namespace GYM.Domain.ValueObjects;

public sealed record DateOfBirth
{
    public DateOnly Value { get; private set; }

    public const int MaxAge = 120;

    private DateOfBirth(DateOnly value)
    {
        Value = value;
    }

    public static Result<DateOfBirth> Create(DateOnly value)
    {
        // Check if the given date is in the future
        if (value > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<DateOfBirth>(DomainErrors.ValueObjects.DateOfBirth.InFuture);

        // Chekc if the given date is more than 120 years ago
        if (value < DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-120)))
            return Result.Failure<DateOfBirth>(DomainErrors.ValueObjects.DateOfBirth.TooOld);

        return Result.Success(new DateOfBirth(value));
    }

    /// <summary>
    /// Creates an Id object from a database value without validation.
    /// This method must be only used when retrieving the value from the database, as it bypasses the validation logic.
    /// </summary>
    /// <param name="value">
    /// The value to create the object from.
    /// </param>
    /// <returns>
    /// The created object.
    /// </returns>
    public static DateOfBirth FromDatabase(DateOnly value)
        => new DateOfBirth(value);
}
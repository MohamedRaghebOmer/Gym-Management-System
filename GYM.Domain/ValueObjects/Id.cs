using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;

namespace GYM.Domain.ValueObjects;

public sealed record Id : ValueObject
{
    public int Value { get; private set; }

    private Id(int value)
    {
        Value = value;
    }

    public static Result<Id> Create(int value)
    {
        if (value <= 0)
        {
            return Result.Failure<Id>(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);
        }

        return new Id(value);
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
    public static Id FromDatabase(int value)
        => new Id(value);
}
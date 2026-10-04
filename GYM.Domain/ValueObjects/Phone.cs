using GYM.Domain.Errors;
using GYM.Domain.Shared;

namespace GYM.Domain.ValueObjects;

public sealed record Phone
{
    public string Value { get; private set; }

    public const int MaxLength = 15;
    public const int MinLength = 7;

    private Phone(string value)
    {
        Value = value;
    }

    public static Result<Phone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Phone>(DomainErrors.ValueObjects.Phone.Empty);

        value = value.Trim();

        if (value.Length is < MinLength or > MaxLength)
            return Result.Failure<Phone>(DomainErrors.ValueObjects.Phone.InvalidLength);

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\+[1-9]\d{7,14}$"))
            return Result.Failure<Phone>(DomainErrors.ValueObjects.Phone.InvalidFormat);

        return new Phone(value);
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
    public static Phone FromDatabase(string value)
        => new Phone(value);
}
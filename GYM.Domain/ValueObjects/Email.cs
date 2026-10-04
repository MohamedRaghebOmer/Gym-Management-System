using GYM.Domain.Errors;
using GYM.Domain.Shared;

namespace GYM.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; private set; }

    public const int MaxLength = 320; // Maximum length for an email address as per RFC 5321

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(DomainErrors.ValueObjects.Email.Empty);

        value = value.Trim();

        if (value.Length > MaxLength)
            return Result.Failure<Email>(DomainErrors.ValueObjects.Email.TooLong);

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result.Failure<Email>(DomainErrors.ValueObjects.Email.InvalidFormat);

        return new Email(value);
    }

    /// <summary>
    /// Creates an Email object from a database value without validation.
    /// This method must be only used when retrieving the value from the database, as it bypasses the validation logic.
    /// </summary>
    /// <param name="value">
    /// The value to create the object from.
    /// </param>
    /// <returns>
    /// The created object.
    /// </returns>
    public static Email FromDatabase(string value)
        => new Email(value);
}
using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class User : Entity
{
    public static class Constants
    {
        public const int UsernameMinLength = 2;
        public const int UsernameMaxLength = 50;
        public const int PasswordHashMaxLength = 500;
    }

    private User(Id personId, string username, string passwordHash, Id roleId)
    {
        PersonId = personId;
        Username = username;
        PasswordHash = passwordHash;
        RoleId = roleId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    private User() { } // For EF Core

    public Id PersonId { get; private set; } = default!;
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Id RoleId { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties (optional)
    public Person? Person { get; private set; }
    public Domain.Entities.Role? Role { get; private set; }

    public static Result<User> Create(
        Id personId,
        string username,
        string passwordHash,
        Id roleId)
    {
        username = username.Trim();

        var result = Validate(username, passwordHash);
        if (result.IsFailure)
            return Result.Failure<User>(result.Error);

        return new User(personId, username, passwordHash, roleId);
    }

    public Result Update(
        string username,
        string passwordHash,
        bool isActive)
    {
        username = username.Trim();

        var result = Validate(username, passwordHash);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Username = username;
        PasswordHash = passwordHash;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(string username, string? passwordHash)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(username))
            errors.Add(DomainErrors.Entities.User.Username.Empty);

        if (!string.IsNullOrWhiteSpace(username) && username.Length < Constants.UsernameMinLength)
            errors.Add(DomainErrors.Entities.User.Username.TooShort);

        if (!string.IsNullOrWhiteSpace(username) && username.Length > Constants.UsernameMaxLength)
            errors.Add(DomainErrors.Entities.User.Username.TooLong);

        if (string.IsNullOrWhiteSpace(passwordHash))
            errors.Add(DomainErrors.Entities.User.Password.Empty);

        if (!string.IsNullOrWhiteSpace(passwordHash) && passwordHash.Length > Constants.PasswordHashMaxLength)
            errors.Add(DomainErrors.Entities.User.Password.TooLong);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

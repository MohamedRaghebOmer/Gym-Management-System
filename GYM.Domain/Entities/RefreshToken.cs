using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class RefreshToken : Entity
{
    public static class Constants
    {
        public const int TokenMaxLength = 500;
    }

    private RefreshToken(
        Id userId,
        string token,
        DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    private RefreshToken() { } // For EF Core

    public Id UserId { get; private set; } = default!;
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? RevokedAt { get; private set; }

    public User? User { get; private set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    public static Result<RefreshToken> Create(
        Id userId,
        string token,
        DateTime expiresAt)
    {
        token = token.Trim();

        var result = Validate(token, expiresAt);
        if (result.IsFailure)
            return Result.Failure<RefreshToken>(result.Error);

        return new RefreshToken(userId, token, expiresAt);
    }

    public Result Update(
        string token,
        DateTime expiresAt)
    {
        token = token.Trim();

        var result = Validate(token, expiresAt);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Token = token;
        ExpiresAt = expiresAt;

        return Result.Success();
    }

    public Result Revoke()
    {
        if (RevokedAt is not null)
            return Result.Failure(DomainErrors.Entities.RefreshToken.AlreadyRevoked);

        RevokedAt = DateTime.UtcNow;
        return Result.Success();
    }

    private static Result Validate(string token, DateTime expiresAt)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(token))
            errors.Add(DomainErrors.Entities.RefreshToken.Token.Empty);
        else if (token.Length > Constants.TokenMaxLength)
            errors.Add(DomainErrors.Entities.RefreshToken.Token.TooLong);

        if (expiresAt == default)
            errors.Add(DomainErrors.Entities.RefreshToken.ExpiresAt.Required);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

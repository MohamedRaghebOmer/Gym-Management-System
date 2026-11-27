using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class RefreshTokenTests
{
    private static readonly Id UserId = Id.FromDatabase(5);

    // A fixed future expiry: 2027-01-01 00:00:00 UTC — always in the future relative to test execution
    private static readonly DateTime FutureExpiry = new(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // A fixed past expiry
    private static readonly DateTime PastExpiry = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    #region Create

    [Fact]
    public void Create_ValidData_ReturnsSuccessAndSetsProperties()
    {
        // Arrange
        var rawToken = "  my-secure-refresh-token  ";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var result = RefreshToken.Create(UserId, rawToken, FutureExpiry);

        var afterCreation = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();

        var entity = result.Value;
        entity.UserId.Should().Be(UserId);
        entity.Token.Should().Be("my-secure-refresh-token"); // trimmed
        entity.ExpiresAt.Should().Be(FutureExpiry);
        entity.RevokedAt.Should().BeNull();
        entity.CreatedAt.Should()
            .BeOnOrAfter(beforeCreation)
            .And.BeOnOrBefore(afterCreation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_TokenIsEmpty_ReturnsValidationFailure(string token)
    {
        // Act
        var result = RefreshToken.Create(UserId, token, FutureExpiry);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.RefreshToken.Token.Empty);
    }

    [Fact]
    public void Create_TokenExceedsMaxLength_ReturnsValidationFailure()
    {
        // Arrange
        var longToken = new string('x', RefreshToken.Constants.TokenMaxLength + 1);

        // Act
        var result = RefreshToken.Create(UserId, longToken, FutureExpiry);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.RefreshToken.Token.TooLong);
    }

    [Fact]
    public void Create_TokenAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxLengthToken = new string('x', RefreshToken.Constants.TokenMaxLength);

        // Act
        var result = RefreshToken.Create(UserId, maxLengthToken, FutureExpiry);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be(maxLengthToken);
    }

    [Fact]
    public void Create_ExpiresAtIsDefault_ReturnsValidationFailure()
    {
        // Act
        var result = RefreshToken.Create(UserId, "some-token", default);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.RefreshToken.ExpiresAt.Required);
    }

    [Fact]
    public void Create_MultipleInvalidFields_ReturnsAllValidationErrors()
    {
        // Act
        var result = RefreshToken.Create(UserId, "", default);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().BeEquivalentTo(new[]
        {
            DomainErrors.Entities.RefreshToken.Token.Empty,
            DomainErrors.Entities.RefreshToken.ExpiresAt.Required
        });
    }

    #endregion

    #region IsActive

    [Fact]
    public void IsActive_TokenNotExpiredAndNotRevoked_ReturnsTrue()
    {
        // Arrange
        var entity = CreateValidEntity(FutureExpiry);

        // Assert
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_TokenExpired_ReturnsFalse()
    {
        // Arrange
        var entity = CreateValidEntity(PastExpiry);

        // Assert
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_TokenRevoked_ReturnsFalse()
    {
        // Arrange
        var entity = CreateValidEntity(FutureExpiry);
        entity.Revoke();

        // Assert
        entity.IsActive.Should().BeFalse();
    }

    #endregion

    #region Revoke

    [Fact]
    public void Revoke_ActiveToken_SetsRevokedAtAndReturnsSuccess()
    {
        // Arrange
        var entity = CreateValidEntity(FutureExpiry);
        var beforeRevoke = DateTime.UtcNow;

        // Act
        var result = entity.Revoke();

        var afterRevoke = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.RevokedAt.Should().NotBeNull();
        entity.RevokedAt!.Value.Should()
            .BeOnOrAfter(beforeRevoke)
            .And.BeOnOrBefore(afterRevoke);
    }

    [Fact]
    public void Revoke_AlreadyRevokedToken_ReturnsFailure()
    {
        // Arrange
        var entity = CreateValidEntity(FutureExpiry);
        entity.Revoke();

        // Act
        var result = entity.Revoke();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Entities.RefreshToken.AlreadyRevoked);
    }

    #endregion

    private static RefreshToken CreateValidEntity(DateTime expiresAt)
    {
        var result = RefreshToken.Create(UserId, "valid-token", expiresAt);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}

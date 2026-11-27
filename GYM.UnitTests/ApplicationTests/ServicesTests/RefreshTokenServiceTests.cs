using FluentAssertions;
using GYM.Application.DTOs.RefreshToken;
using GYM.Application.Errors;
using GYM.Application.Services;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _repoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<RefreshTokenService>> _loggerMock = new();

    private readonly RefreshTokenService _sut;

    private static readonly Id UserId = Id.FromDatabase(5);

    // Fixed future expiry — always in the future relative to test execution
    private static readonly DateTime FutureExpiry = new(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public RefreshTokenServiceTests()
    {
        _sut = new RefreshTokenService(
            _repoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_InvalidUserId_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var dto = CreateValidDto() with { UserId = 0 };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);

        _userRepoMock.Verify(
            x => x.ExistsAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repoMock.Verify(x => x.Add(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UserNotFound_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidDto();

        _userRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == UserId), cancellationToken))
            .ReturnsAsync(false);
        _repoMock
            .Setup(x => x.ExistsByTokenAsync(dto.Token, cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(ServiceErrors.Common.NotFound);

        _repoMock.Verify(x => x.Add(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateToken_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidDto();

        _userRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == UserId), cancellationToken))
            .ReturnsAsync(true);
        _repoMock
            .Setup(x => x.ExistsByTokenAsync(dto.Token.Trim(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(ServiceErrors.RefreshToken.Duplicate);

        _repoMock.Verify(x => x.Add(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidDomainData_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange — empty token passes the outer uniqueness check (skipped for whitespace),
        //           but domain validation rejects it
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidDto() with { Token = "  " };

        _userRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == UserId), cancellationToken))
            .ReturnsAsync(true);
        // ExistsByTokenAsync is NOT called because the trimmed token is whitespace-only

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.RefreshToken.Token.Empty);

        _repoMock.Verify(x => x.Add(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SaveChangesThrows_ReturnsUnexpectedWithoutLeakingException()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidDto();

        _userRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == UserId), cancellationToken))
            .ReturnsAsync(true);
        _repoMock
            .Setup(x => x.ExistsByTokenAsync(dto.Token.Trim(), cancellationToken))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.Unexpected);

        // Add was still called before the exception
        _repoMock.Verify(x => x.Add(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CancellationTokenIsForwardedToAllDependencies()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var dto = CreateValidDto();

        _userRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == UserId), cancellationToken))
            .ReturnsAsync(true);
        _repoMock
            .Setup(x => x.ExistsByTokenAsync(dto.Token.Trim(), cancellationToken))
            .ReturnsAsync(false);

        // Act
        await _sut.CreateAsync(dto, cancellationToken);

        // Assert — each dependency received the same token
        _userRepoMock.Verify(
            x => x.ExistsAsync(It.IsAny<Id>(), cancellationToken),
            Times.Once);
        _repoMock.Verify(
            x => x.ExistsByTokenAsync(It.IsAny<string>(), cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    #endregion

    private static CreateRefreshTokenDto CreateValidDto() => new()
    {
        UserId = UserId.Value,
        Token = "  valid-refresh-token-value  ",
        ExpiresAt = FutureExpiry
    };
}

using FluentAssertions;
using GYM.Application.Abstractions;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GYM.UnitTests.ApplicationTests.AbstractionsTests;

public sealed class ServiceBaseTests
{
    private readonly Mock<IRepository<TestEntity>> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<ServiceBase<TestEntity, TestResponseDto, TestCreateDto, TestUpdateDto>>> _loggerMock = new();

    private readonly TestService _sut;

    public ServiceBaseTests()
    {
        _sut = new TestService(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_WhenIdIsInvalid_ReturnsFailure_AndDoesNotQueryRepository(int id)
    {
        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _sut.GetByIdAsync(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.NotFound);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ReturnsMappedResponseDto_AndForwardsCancellationToken()
    {
        // Arrange
        const int id = 10;
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var entity = new TestEntity("Strength Training");

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<Id>(),
                cancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.GetByIdAsync(id, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new TestResponseDto("Strength Training"));

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Id>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepositoryThrows_ReturnsUnexpectedFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository failure."));

        // Act
        var result = await _sut.GetByIdAsync(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.UnExcepected);
    }

    [Fact]
    public async Task GetAllAsync_WhenEntitiesExist_ReturnsMappedDtosInSourceOrder()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new("Strength Training"),
            new("Cardio")
        };

        _repositoryMock
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(
            new[]
            {
                new TestResponseDto("Strength Training"),
                new TestResponseDto("Cardio")
            },
            options => options.WithStrictOrdering());

        _repositoryMock.Verify(
            repository => repository.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryReturnsNoEntities_ReturnsAnEmptySuccessfulCollection()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestEntity>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrows_ReturnsUnexpectedFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository failure."));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.UnExcepected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_WhenIdIsInvalid_ReturnsFailure_AndDoesNotQueryRepository(int id)
    {
        // Act
        var result = await _sut.ExistsAsync(id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);

        _repositoryMock.Verify(
            repository => repository.ExistsAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsAsync_WhenIdIsValid_ReturnsRepositoryResult(bool exists)
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

        // Act
        var result = await _sut.ExistsAsync(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(exists);

        _repositoryMock.Verify(
            repository => repository.ExistsAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenRepositoryThrows_ReturnsUnexpectedFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository failure."));

        // Act
        var result = await _sut.ExistsAsync(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.UnExcepected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_WhenIdIsInvalid_ReturnsFailure_WithoutRemovingOrSaving(int id)
    {
        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityDoesNotExist_ReturnsNotFound_AndDoesNotSaveChanges()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.RemoveAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.NotFound);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityIsRemoved_ReturnsSuccess_AndCommitsChanges()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock
            .Setup(repository => repository.RemoveAsync(
                It.IsAny<Id>(),
                cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(10, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                It.IsAny<Id>(),
                cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRepositoryThrows_ReturnsUnexpectedFailure_AndDoesNotSaveChanges()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.RemoveAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository failure."));

        // Act
        var result = await _sut.DeleteAsync(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.UnExcepected);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenSavingChangesThrows_ReturnsUnexpectedFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.RemoveAsync(
                It.IsAny<Id>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unit of work failure."));

        // Act
        var result = await _sut.DeleteAsync(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.UnExcepected);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class TestService(
        IRepository<TestEntity> repository,
        IUnitOfWork unitOfWork,
        ILogger<ServiceBase<TestEntity, TestResponseDto, TestCreateDto, TestUpdateDto>> logger)
        : ServiceBase<TestEntity, TestResponseDto, TestCreateDto, TestUpdateDto>(
            repository,
            unitOfWork,
            logger)
    {
        public override Task<Result<int>> CreateAsync(
            TestCreateDto dto,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<int>(ServiceErrors.Common.UnExcepected));

        public override Task<Result> UpdateAsync(
            int id,
            TestUpdateDto dto,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure(ServiceErrors.Common.UnExcepected));
    }

    public sealed class TestEntity(string name) : Entity
    {
        public string Name { get; } = name;
    }

    public sealed record TestResponseDto(string Name)
        : IResponseDto<TestEntity, TestResponseDto>
    {
        public static TestResponseDto FromEntity(TestEntity entity)
            => new(entity.Name);
    }

    public sealed record TestCreateDto : ICreateDto;

    public sealed record TestUpdateDto : IUpdateDto;
}

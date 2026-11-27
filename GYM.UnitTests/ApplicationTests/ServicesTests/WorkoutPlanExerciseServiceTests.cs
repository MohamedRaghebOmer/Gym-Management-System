using FluentAssertions;
using GYM.Application.DTOs.WorkoutPlanExercise;
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

public sealed class WorkoutPlanExerciseServiceTests
{
    private readonly Mock<IWorkoutPlanExerciseRepository> _repoMock = new();
    private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepoMock = new();
    private readonly Mock<IExerciseRepository> _exerciseRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<WorkoutPlanExerciseService>> _loggerMock = new();

    private readonly WorkoutPlanExerciseService _sut;

    private static readonly Id WorkoutPlanId = Id.FromDatabase(10);
    private static readonly Id ExerciseId = Id.FromDatabase(20);

    public WorkoutPlanExerciseServiceTests()
    {
        _sut = new WorkoutPlanExerciseService(
            _repoMock.Object,
            _workoutPlanRepoMock.Object,
            _exerciseRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_InvalidIds_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var dto = CreateValidCreateDto() with
        {
            WorkoutPlanId = 0,
            ExerciseId = -1
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().HaveCount(2);
        validationErrors.Errors.Should().AllSatisfy(
            e => e.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero));

        _workoutPlanRepoMock.Verify(
            x => x.ExistsAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _exerciseRepoMock.Verify(
            x => x.ExistsAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repoMock.Verify(x => x.Add(It.IsAny<WorkoutPlanExercise>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WorkoutPlanNotFound_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidCreateDto();

        _workoutPlanRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == WorkoutPlanId), cancellationToken))
            .ReturnsAsync(false);
        _exerciseRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == ExerciseId), cancellationToken))
            .ReturnsAsync(true);
        _repoMock
            .Setup(x => x.ExistsAsync(
                It.Is<Id>(id => id == WorkoutPlanId),
                It.Is<Id>(id => id == ExerciseId),
                cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(ServiceErrors.Common.NotFound);

        _repoMock.Verify(x => x.Add(It.IsAny<WorkoutPlanExercise>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ExerciseNotFound_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidCreateDto();

        _workoutPlanRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == WorkoutPlanId), cancellationToken))
            .ReturnsAsync(true);
        _exerciseRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == ExerciseId), cancellationToken))
            .ReturnsAsync(false);
        _repoMock
            .Setup(x => x.ExistsAsync(
                It.Is<Id>(id => id == WorkoutPlanId),
                It.Is<Id>(id => id == ExerciseId),
                cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(ServiceErrors.Common.NotFound);

        _repoMock.Verify(x => x.Add(It.IsAny<WorkoutPlanExercise>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateWorkoutPlanAndExercise_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidCreateDto();

        SetupExistingParentsAndDuplicate(
            workoutPlanExists: true,
            exerciseExists: true,
            alreadyExists: true,
            cancellationToken);

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(ServiceErrors.WorkoutPlanExercise.AlreadyExists);

        _repoMock.Verify(x => x.Add(It.IsAny<WorkoutPlanExercise>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidDomainData_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidCreateDto() with
        {
            Sets = 0,
            Weight = -10m
        };

        SetupExistingParentsAndDuplicate(
            workoutPlanExists: true,
            exerciseExists: true,
            alreadyExists: false,
            cancellationToken);

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().BeEquivalentTo(new[]
        {
            DomainErrors.Entities.WorkoutPlanExercise.Sets.LessThanOrEqualToZero,
            DomainErrors.Entities.WorkoutPlanExercise.Weight.Negative
        });

        _repoMock.Verify(x => x.Add(It.IsAny<WorkoutPlanExercise>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SaveChangesThrows_AddsMappedEntityAndReturnsUnexpected()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidCreateDto();

        SetupExistingParentsAndDuplicate(
            workoutPlanExists: true,
            exerciseExists: true,
            alreadyExists: false,
            cancellationToken);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.CreateAsync(dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.Unexpected);

        _repoMock.Verify(
            x => x.Add(It.Is<WorkoutPlanExercise>(wpe =>
                wpe.WorkoutPlanId == WorkoutPlanId &&
                wpe.ExerciseId == ExerciseId &&
                wpe.Sets == dto.Sets &&
                wpe.Reps == dto.Reps &&
                wpe.Weight == dto.Weight &&
                wpe.RestSeconds == dto.RestSeconds &&
                wpe.DisplayOrder == dto.DisplayOrder &&
                wpe.Notes == "Controlled tempo")),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutQueryingOrSaving()
    {
        // Arrange
        var dto = CreateValidUpdateDto();

        // Act
        var result = await _sut.UpdateAsync(0, dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);

        _repoMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFoundWithoutSaving()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = CreateValidUpdateDto();

        _repoMock
            .Setup(x => x.GetByIdAsync(It.Is<Id>(id => id.Value == 15), cancellationToken))
            .ReturnsAsync((WorkoutPlanExercise?)null);

        // Act
        var result = await _sut.UpdateAsync(15, dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.NotFound);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidDomainData_ReturnsValidationFailureWithoutSaving()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entity = CreateValidEntity();
        var dto = CreateValidUpdateDto() with
        {
            Sets = 0,
            RestSeconds = -5
        };

        _repoMock
            .Setup(x => x.GetByIdAsync(It.Is<Id>(id => id.Value == 5), cancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.UpdateAsync(5, dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().BeEquivalentTo(new[]
        {
            DomainErrors.Entities.WorkoutPlanExercise.Sets.LessThanOrEqualToZero,
            DomainErrors.Entities.WorkoutPlanExercise.RestSeconds.Negative
        });

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEntityAndSavesChanges()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entity = CreateValidEntity();
        var dto = CreateValidUpdateDto();

        _repoMock
            .Setup(x => x.GetByIdAsync(It.Is<Id>(id => id.Value == 7), cancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.UpdateAsync(7, dto, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Sets.Should().Be(dto.Sets);
        entity.Reps.Should().Be(dto.Reps);
        entity.Weight.Should().Be(dto.Weight);
        entity.RestSeconds.Should().Be(dto.RestSeconds);
        entity.DisplayOrder.Should().Be(dto.DisplayOrder);
        entity.Notes.Should().Be("Updated notes");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ClearingNullableFields_SetsNullablePropertiesToNullAndSavesChanges()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entity = CreateValidEntity();
        var dto = CreateValidUpdateDto() with
        {
            Reps = null,
            Weight = null,
            RestSeconds = null,
            Notes = null
        };

        _repoMock
            .Setup(x => x.GetByIdAsync(It.Is<Id>(id => id.Value == 7), cancellationToken))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.UpdateAsync(7, dto, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Reps.Should().BeNull();
        entity.Weight.Should().BeNull();
        entity.RestSeconds.Should().BeNull();
        entity.Notes.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_SaveChangesThrows_ReturnsUnexpected()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var entity = CreateValidEntity();
        var dto = CreateValidUpdateDto();

        _repoMock
            .Setup(x => x.GetByIdAsync(It.Is<Id>(id => id.Value == 7), cancellationToken))
            .ReturnsAsync(entity);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.UpdateAsync(7, dto, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.Unexpected);
    }

    #endregion

    private void SetupExistingParentsAndDuplicate(
        bool workoutPlanExists,
        bool exerciseExists,
        bool alreadyExists,
        CancellationToken cancellationToken)
    {
        _workoutPlanRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == WorkoutPlanId), cancellationToken))
            .ReturnsAsync(workoutPlanExists);
        _exerciseRepoMock
            .Setup(x => x.ExistsAsync(It.Is<Id>(id => id == ExerciseId), cancellationToken))
            .ReturnsAsync(exerciseExists);
        _repoMock
            .Setup(x => x.ExistsAsync(
                It.Is<Id>(id => id == WorkoutPlanId),
                It.Is<Id>(id => id == ExerciseId),
                cancellationToken))
            .ReturnsAsync(alreadyExists);
    }

    private static CreateWorkoutPlanExerciseDto CreateValidCreateDto() => new()
    {
        WorkoutPlanId = WorkoutPlanId.Value,
        ExerciseId = ExerciseId.Value,
        Sets = 4,
        Reps = 10,
        Weight = 80.50m,
        RestSeconds = 90,
        DisplayOrder = 1,
        Notes = "  Controlled tempo  "
    };

    private static WorkoutPlanExerciseUpdateDto CreateValidUpdateDto() => new()
    {
        Sets = 5,
        Reps = 8,
        Weight = 95.00m,
        RestSeconds = 120,
        DisplayOrder = 2,
        Notes = "  Updated notes  "
    };

    private static WorkoutPlanExercise CreateValidEntity()
    {
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 3,
            reps: 12,
            weight: 60m,
            restSeconds: 60,
            displayOrder: 1,
            notes: "Initial notes");

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}

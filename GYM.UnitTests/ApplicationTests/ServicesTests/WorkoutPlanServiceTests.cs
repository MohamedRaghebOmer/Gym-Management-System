using FluentAssertions;
using GYM.Application.DTOs.WorkoutPlan;
using GYM.Application.Errors;
using GYM.Application.Services;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class WorkoutPlanServiceTests
{
    private readonly Mock<IWorkoutPlanRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<WorkoutPlanService>> _loggerMock = new();

    private readonly WorkoutPlanService _sut;

    private static readonly Id MemberId = Id.FromDatabase(10);
    private static readonly Id TrainerId = Id.FromDatabase(20);

    private static readonly DateOnly StartDate = new(2026, 11, 7);
    private static readonly DateOnly EndDate = new(2026, 12, 7);

    public WorkoutPlanServiceTests()
    {
        _sut = new WorkoutPlanService(
            _repoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenIdsAreInvalid()
    {
        // Arrange
        var dto = CreateValidCreateDto() with
        {
            MemberId = 0,
            TrainerId = -1
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();

        var validationErrors = (ValidationErrors)result.Error;
        validationErrors.Errors.Should().HaveCount(2);

        _repoMock.Verify(
            x => x.Add(It.IsAny<WorkoutPlan>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenWorkoutPlanDataIsInvalid()
    {
        // Arrange
        var dto = CreateValidCreateDto() with
        {
            Name = "",
            EndDate = StartDate.AddDays(-1)
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();

        var validationErrors = (ValidationErrors)result.Error;
        validationErrors.Errors.Should().HaveCount(2);

        _repoMock.Verify(
            x => x.Add(It.IsAny<WorkoutPlan>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnUnexpected_WhenSaveChangesThrows()
    {
        // Arrange
        var dto = CreateValidCreateDto();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.Unexpected);

        _repoMock.Verify(
            x => x.Add(It.Is<WorkoutPlan>(p =>
                p.MemberId == MemberId &&
                p.TrainerId == TrainerId &&
                p.Name == dto.Name)),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenIdIsInvalid()
    {
        // Arrange
        var dto = CreateValidUpdateDto();

        // Act
        var result = await _sut.UpdateAsync(0, dto);

        // Assert
        result.IsFailure.Should().BeTrue();

        _repoMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenWorkoutPlanDoesNotExist()
    {
        // Arrange
        var id = 100;
        var dto = CreateValidUpdateDto();

        _repoMock
            .Setup(x => x.GetByIdAsync(
                It.Is<Id>(i => i.Value == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkoutPlan?)null);

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.NotFound);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenWorkoutPlanDataIsInvalid()
    {
        // Arrange
        var id = 100;
        var workoutPlan = CreateValidWorkoutPlan();

        var dto = CreateValidUpdateDto() with
        {
            Name = "",
            EndDate = StartDate.AddDays(-1)
        };

        _repoMock
            .Setup(x => x.GetByIdAsync(
                It.Is<Id>(i => i.Value == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workoutPlan);

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();

        workoutPlan.Name.Should().Be("Strength Program");
        workoutPlan.StartDate.Should().Be(StartDate);
        workoutPlan.EndDate.Should().Be(EndDate);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateWorkoutPlan_WhenDataIsValid()
    {
        // Arrange
        var id = 100;
        var workoutPlan = CreateValidWorkoutPlan();

        var dto = CreateValidUpdateDto() with
        {
            Name = "Advanced Strength Program",
            Description = "Updated workout description.",
            StartDate = StartDate.AddDays(5),
            EndDate = EndDate.AddDays(5),
            IsActive = false
        };

        _repoMock
            .Setup(x => x.GetByIdAsync(
                It.Is<Id>(i => i.Value == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workoutPlan);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        result.IsFailure.Should().BeFalse();

        workoutPlan.Name.Should().Be(dto.Name);
        workoutPlan.Description.Should().Be(dto.Description);
        workoutPlan.StartDate.Should().Be(dto.StartDate);
        workoutPlan.EndDate.Should().Be(dto.EndDate);
        workoutPlan.IsActive.Should().Be(dto.IsActive);
        workoutPlan.UpdatedAt.Should().NotBeNull();

        _repoMock.Verify(
            x => x.GetByIdAsync(
                It.Is<Id>(i => i.Value == id),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUnexpected_WhenSaveChangesThrows()
    {
        // Arrange
        var id = 100;
        var workoutPlan = CreateValidWorkoutPlan();
        var dto = CreateValidUpdateDto();

        _repoMock
            .Setup(x => x.GetByIdAsync(
                It.Is<Id>(i => i.Value == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workoutPlan);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.Common.Unexpected);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helpers

    private static CreateWorkoutPlanDto CreateValidCreateDto()
    {
        return new CreateWorkoutPlanDto
        {
            MemberId = MemberId.Value,
            TrainerId = TrainerId.Value,
            Name = "Strength Program",
            Description = "Build strength and endurance.",
            StartDate = StartDate,
            EndDate = EndDate
        };
    }

    private static UpdateWorkoutPlanDto CreateValidUpdateDto()
    {
        return new UpdateWorkoutPlanDto
        {
            Name = "Strength Program",
            Description = "Build strength and endurance.",
            StartDate = StartDate,
            EndDate = EndDate,
            IsActive = true
        };
    }

    private static WorkoutPlan CreateValidWorkoutPlan()
    {
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            "Strength Program",
            "Build strength and endurance.",
            StartDate,
            EndDate);

        result.IsFailure.Should().BeFalse();

        return result.Value;
    }

    #endregion
}
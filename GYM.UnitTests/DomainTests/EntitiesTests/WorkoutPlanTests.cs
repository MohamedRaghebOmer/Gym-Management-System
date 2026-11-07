using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class WorkoutPlanTests
{
    private static readonly Id MemberId = Id.FromDatabase(10);
    private static readonly Id TrainerId = Id.FromDatabase(20);

    private static readonly DateOnly StartDate = new(2026, 11, 7);
    private static readonly DateOnly EndDate = new(2026, 12, 7);

    #region Create

    [Fact]
    public void Create_ShouldCreateWorkoutPlan_WhenDataIsValid()
    {
        // Arrange
        var name = "  Strength Program  ";
        var description = "  Build strength and endurance.  ";

        var beforeCreation = DateTime.UtcNow;

        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            name,
            description,
            StartDate,
            EndDate);

        var afterCreation = DateTime.UtcNow;

        // Assert
        result.IsFailure.Should().BeFalse();

        var workoutPlan = result.Value;

        workoutPlan.MemberId.Should().Be(MemberId);
        workoutPlan.TrainerId.Should().Be(TrainerId);
        workoutPlan.Name.Should().Be("Strength Program");
        workoutPlan.Description.Should().Be("Build strength and endurance.");
        workoutPlan.StartDate.Should().Be(StartDate);
        workoutPlan.EndDate.Should().Be(EndDate);
        workoutPlan.IsActive.Should().BeTrue();

        workoutPlan.CreatedAt.Should()
            .BeOnOrAfter(beforeCreation)
            .And.BeOnOrBefore(afterCreation);

        workoutPlan.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenDescriptionIsNull()
    {
        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            "Strength Program",
            null,
            StartDate,
            EndDate);

        // Assert
        result.IsFailure.Should().BeFalse();
        result.Value.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenNameIsEmpty(string name)
    {
        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            name,
            null,
            StartDate,
            EndDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
    }

    [Fact]
    public void Create_ShouldFail_WhenNameExceedsMaxLength()
    {
        // Arrange
        var name = new string('A', WorkoutPlan.Constants.NameMaxLength + 1);

        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            name,
            null,
            StartDate,
            EndDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
    }

    [Fact]
    public void Create_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var description = new string('A', WorkoutPlan.Constants.DescriptionMaxLength + 1);

        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            "Strength Program",
            description,
            StartDate,
            EndDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
    }

    [Fact]
    public void Create_ShouldFail_WhenStartDateIsDefault()
    {
        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            "Strength Program",
            null,
            default,
            EndDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
    }

    [Fact]
    public void Create_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var endDate = StartDate.AddDays(-1);

        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            "Strength Program",
            null,
            StartDate,
            endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenEndDateEqualsStartDate()
    {
        // Act
        var result = WorkoutPlan.Create(
            MemberId,
            TrainerId,
            "Strength Program",
            null,
            StartDate,
            StartDate);

        // Assert
        result.IsFailure.Should().BeFalse();
        result.Value.EndDate.Should().Be(StartDate);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_ShouldUpdateWorkoutPlan_WhenDataIsValid()
    {
        // Arrange
        var workoutPlan = CreateValidWorkoutPlan();

        var newName = "  Advanced Strength Program  ";
        var newDescription = "  Updated workout description.  ";
        var newStartDate = StartDate.AddDays(5);
        var newEndDate = EndDate.AddDays(5);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = workoutPlan.Update(
            newName,
            newDescription,
            newStartDate,
            newEndDate,
            false);

        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.IsFailure.Should().BeFalse();

        workoutPlan.Name.Should().Be("Advanced Strength Program");
        workoutPlan.Description.Should().Be("Updated workout description.");
        workoutPlan.StartDate.Should().Be(newStartDate);
        workoutPlan.EndDate.Should().Be(newEndDate);
        workoutPlan.IsActive.Should().BeFalse();

        workoutPlan.UpdatedAt.Should().NotBeNull();

        workoutPlan.UpdatedAt!.Value.Should()
            .BeOnOrAfter(beforeUpdate)
            .And.BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public void Update_ShouldFailAndPreserveExistingData_WhenDataIsInvalid()
    {
        // Arrange
        var workoutPlan = CreateValidWorkoutPlan();

        var originalName = workoutPlan.Name;
        var originalDescription = workoutPlan.Description;
        var originalStartDate = workoutPlan.StartDate;
        var originalEndDate = workoutPlan.EndDate;
        var originalIsActive = workoutPlan.IsActive;
        var originalUpdatedAt = workoutPlan.UpdatedAt;

        // Act
        var result = workoutPlan.Update(
            new string('A', WorkoutPlan.Constants.NameMaxLength + 1),
            "Updated description",
            StartDate.AddDays(5),
            EndDate.AddDays(5),
            false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();

        workoutPlan.Name.Should().Be(originalName);
        workoutPlan.Description.Should().Be(originalDescription);
        workoutPlan.StartDate.Should().Be(originalStartDate);
        workoutPlan.EndDate.Should().Be(originalEndDate);
        workoutPlan.IsActive.Should().Be(originalIsActive);
        workoutPlan.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Update_ShouldFail_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var workoutPlan = CreateValidWorkoutPlan();

        // Act
        var result = workoutPlan.Update(
            "Updated Program",
            null,
            StartDate,
            StartDate.AddDays(-1),
            true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
    }

    [Fact]
    public void Update_ShouldAllowNullEndDate()
    {
        // Arrange
        var workoutPlan = CreateValidWorkoutPlan();

        // Act
        var result = workoutPlan.Update(
            "Updated Program",
            null,
            StartDate,
            null,
            true);

        // Assert
        result.IsFailure.Should().BeFalse();

        workoutPlan.EndDate.Should().BeNull();
        workoutPlan.Description.Should().BeNull();
    }

    #endregion

    #region Helpers

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
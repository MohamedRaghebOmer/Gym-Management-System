using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class WorkoutPlanExerciseTests
{
    private static readonly Id WorkoutPlanId = Id.FromDatabase(10);
    private static readonly Id ExerciseId = Id.FromDatabase(20);

    #region Create

    [Fact]
    public void Create_ValidData_ReturnsSuccessAndSetsProperties()
    {
        // Arrange
        byte sets = 4;
        byte? reps = 10;
        decimal? weight = 75.50m;
        short? restSeconds = 90;
        byte displayOrder = 2;
        var notes = "  Keep back straight  ";

        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets,
            reps,
            weight,
            restSeconds,
            displayOrder,
            notes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var entity = result.Value;
        entity.WorkoutPlanId.Should().Be(WorkoutPlanId);
        entity.ExerciseId.Should().Be(ExerciseId);
        entity.Sets.Should().Be(sets);
        entity.Reps.Should().Be(reps);
        entity.Weight.Should().Be(weight);
        entity.RestSeconds.Should().Be(restSeconds);
        entity.DisplayOrder.Should().Be(displayOrder);
        entity.Notes.Should().Be("Keep back straight");
    }

    [Fact]
    public void Create_NullablePropertiesAreNull_ReturnsSuccess()
    {
        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 3,
            reps: null,
            weight: null,
            restSeconds: null,
            displayOrder: 1,
            notes: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reps.Should().BeNull();
        result.Value.Weight.Should().BeNull();
        result.Value.RestSeconds.Should().BeNull();
        result.Value.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_BoundaryValues_ReturnsSuccess()
    {
        // Arrange
        var maxLengthNotes = new string('A', WorkoutPlanExercise.Constants.NotesMaxLength);

        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 1,
            reps: 1,
            weight: 0m,
            restSeconds: 0,
            displayOrder: 0,
            notes: maxLengthNotes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Sets.Should().Be(1);
        result.Value.Reps.Should().Be(1);
        result.Value.Weight.Should().Be(0m);
        result.Value.RestSeconds.Should().Be(0);
        result.Value.DisplayOrder.Should().Be(0);
        result.Value.Notes.Should().Be(maxLengthNotes);
    }

    [Fact]
    public void Create_SetsIsZero_ReturnsValidationFailure()
    {
        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 0,
            reps: 10,
            weight: 50m,
            restSeconds: 60,
            displayOrder: 1,
            notes: null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.WorkoutPlanExercise.Sets.LessThanOrEqualToZero);
    }

    [Fact]
    public void Create_RepsIsZero_ReturnsValidationFailure()
    {
        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 3,
            reps: 0,
            weight: 50m,
            restSeconds: 60,
            displayOrder: 1,
            notes: null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.WorkoutPlanExercise.Reps.LessThanOrEqualToZero);
    }

    [Fact]
    public void Create_WeightIsNegative_ReturnsValidationFailure()
    {
        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 3,
            reps: 10,
            weight: -0.01m,
            restSeconds: 60,
            displayOrder: 1,
            notes: null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.WorkoutPlanExercise.Weight.Negative);
    }

    [Fact]
    public void Create_RestSecondsIsNegative_ReturnsValidationFailure()
    {
        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 3,
            reps: 10,
            weight: 50m,
            restSeconds: -1,
            displayOrder: 1,
            notes: null);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.WorkoutPlanExercise.RestSeconds.Negative);
    }

    [Fact]
    public void Create_NotesExceedsMaxLength_ReturnsValidationFailure()
    {
        // Arrange
        var notes = new string('A', WorkoutPlanExercise.Constants.NotesMaxLength + 1);

        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 3,
            reps: 10,
            weight: 50m,
            restSeconds: 60,
            displayOrder: 1,
            notes: notes);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle()
            .Which.Should().Be(DomainErrors.Entities.WorkoutPlanExercise.Notes.TooLong);
    }

    [Fact]
    public void Create_MultipleInvalidFields_ReturnsAllValidationErrors()
    {
        // Arrange
        var notes = new string('A', WorkoutPlanExercise.Constants.NotesMaxLength + 1);

        // Act
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 0,
            reps: 0,
            weight: -1m,
            restSeconds: -1,
            displayOrder: 1,
            notes: notes);

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().BeEquivalentTo(new[]
        {
            DomainErrors.Entities.WorkoutPlanExercise.Sets.LessThanOrEqualToZero,
            DomainErrors.Entities.WorkoutPlanExercise.Reps.LessThanOrEqualToZero,
            DomainErrors.Entities.WorkoutPlanExercise.Weight.Negative,
            DomainErrors.Entities.WorkoutPlanExercise.RestSeconds.Negative,
            DomainErrors.Entities.WorkoutPlanExercise.Notes.TooLong
        });
    }

    #endregion

    #region Update

    [Fact]
    public void Update_ValidData_UpdatesPropertiesAndTrimsNotes()
    {
        // Arrange
        var entity = CreateValidEntity();

        // Act
        var result = entity.Update(
            sets: 5,
            reps: 12,
            weight: 85.25m,
            restSeconds: 120,
            displayOrder: 3,
            notes: "  Slow eccentric  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Sets.Should().Be(5);
        entity.Reps.Should().Be(12);
        entity.Weight.Should().Be(85.25m);
        entity.RestSeconds.Should().Be(120);
        entity.DisplayOrder.Should().Be(3);
        entity.Notes.Should().Be("Slow eccentric");
    }

    [Fact]
    public void Update_ClearingNullableFields_SetsNullablePropertiesToNull()
    {
        // Arrange
        var entity = CreateValidEntity();

        // Act
        var result = entity.Update(
            sets: 3,
            reps: null,
            weight: null,
            restSeconds: null,
            displayOrder: 2,
            notes: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        entity.Sets.Should().Be(3);
        entity.Reps.Should().BeNull();
        entity.Weight.Should().BeNull();
        entity.RestSeconds.Should().BeNull();
        entity.DisplayOrder.Should().Be(2);
        entity.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_InvalidData_ReturnsValidationFailureWithoutMutatingState()
    {
        // Arrange
        var entity = CreateValidEntity();

        // Act
        var result = entity.Update(
            sets: 0,
            reps: 0,
            weight: -5m,
            restSeconds: -10,
            displayOrder: 9,
            notes: new string('B', WorkoutPlanExercise.Constants.NotesMaxLength + 1));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();

        entity.Sets.Should().Be(4);
        entity.Reps.Should().Be(10);
        entity.Weight.Should().Be(60m);
        entity.RestSeconds.Should().Be(90);
        entity.DisplayOrder.Should().Be(1);
        entity.Notes.Should().Be("Initial notes");
    }

    #endregion

    private static WorkoutPlanExercise CreateValidEntity()
    {
        var result = WorkoutPlanExercise.Create(
            WorkoutPlanId,
            ExerciseId,
            sets: 4,
            reps: 10,
            weight: 60m,
            restSeconds: 90,
            displayOrder: 1,
            notes: "Initial notes");

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}

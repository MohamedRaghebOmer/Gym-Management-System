using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class ExerciseTests
{
    [Fact]
    public void Create_ValidInput_ReturnsExerciseWithTrimmedValuesAndDefaults()
    {
        var gymId = Id.Create(3).Value;

        var result = Exercise.Create(
            gymId,
            "  Bench Press  ",
            "  Chest  ",
            "  Compound movement  ",
            "  Keep your back stable  ",
            "  https://video.example/bench  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.GymId.Should().Be(gymId);
        result.Value.Name.Should().Be("Bench Press");
        result.Value.MuscleGroup.Should().Be("Chest");
        result.Value.Description.Should().Be("Compound movement");
        result.Value.Instructions.Should().Be("Keep your back stable");
        result.Value.VideoUrl.Should().Be("https://video.example/bench");
        result.Value.IsActive.Should().BeTrue();
        result.Value.CreatedAt.Should().NotBe(default);
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyNameAndMuscleGroup_ReturnsValidationFailure()
    {
        var result = Exercise.Create(
            Id.Create(1).Value,
            "   ",
            "   ",
            null,
            null,
            null);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;

        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.Name.Empty.Code);
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.MuscleGroup.Empty.Code);
    }

    [Fact]
    public void Create_TooLongOptionalFields_ReturnsValidationFailure()
    {
        var result = Exercise.Create(
            Id.Create(1).Value,
            "Deadlift",
            "Back",
            new string('d', Exercise.Constants.DescriptionMaxLength + 1),
            new string('i', Exercise.Constants.InstructionsMaxLength + 1),
            new string('u', Exercise.Constants.VideoUrlMaxLength + 1));

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;

        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.Description.TooLong.Code);
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.Instructions.TooLong.Code);
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.VideoUrl.TooLong.Code);
    }

    [Fact]
    public void Create_InvalidVideoUrl_ReturnsValidationFailure()
    {
        var result = Exercise.Create(
            Id.Create(1).Value,
            "Deadlift",
            "Back",
            "desc",
            "instructions",
            "this is a url");

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.VideoUrl.InvalidFormat.Code);
    }

    [Fact]
    public void Update_ValidInput_UpdatesStateAndSetsUpdatedAt()
    {
        var exercise = Exercise.Create(
            Id.Create(4).Value,
            "Squat",
            "Legs",
            "old description",
            "old instructions",
            "https://video.example/old-squat").Value;

        var result = exercise.Update(
            "  Front Squat  ",
            "  Quadriceps  ",
            "  New description  ",
            "  New instructions  ",
            "  https://video.example/front-squat  ",
            false);

        result.IsSuccess.Should().BeTrue();
        exercise.Name.Should().Be("Front Squat");
        exercise.MuscleGroup.Should().Be("Quadriceps");
        exercise.Description.Should().Be("New description");
        exercise.Instructions.Should().Be("New instructions");
        exercise.VideoUrl.Should().Be("https://video.example/front-squat");
        exercise.IsActive.Should().BeFalse();
        exercise.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_InvalidInput_ReturnsValidationFailureAndDoesNotChangeState()
    {
        var exercise = Exercise.Create(
            Id.Create(5).Value,
            "Row",
            "Back",
            "desc",
            "instructions",
            "https://youtube.com").Value;

        var originalName = exercise.Name;
        var originalMuscleGroup = exercise.MuscleGroup;
        var originalDescription = exercise.Description;
        var originalInstructions = exercise.Instructions;
        var originalVideoUrl = exercise.VideoUrl;
        var originalIsActive = exercise.IsActive;

        var result = exercise.Update(
            " ",
            new string('m', Exercise.Constants.MuscleGroupMaxLength + 1),
            exercise.Description,
            exercise.Instructions,
            "not-a-url",
            false);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.Name.Empty.Code);
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.MuscleGroup.TooLong.Code);
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.VideoUrl.InvalidFormat.Code);

        exercise.Name.Should().Be(originalName);
        exercise.MuscleGroup.Should().Be(originalMuscleGroup);
        exercise.Description.Should().Be(originalDescription);
        exercise.Instructions.Should().Be(originalInstructions);
        exercise.VideoUrl.Should().Be(originalVideoUrl);
        exercise.IsActive.Should().Be(originalIsActive);
        exercise.UpdatedAt.Should().BeNull();
    }
}

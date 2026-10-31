using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class TrainerTests
{
    [Fact]
    public void Create_ValidData_ReturnsTrainerWithExpectedState()
    {
        var personId = Id.Create(5).Value;
        var hireDate = DateTime.UtcNow.AddDays(-10);

        var result = Trainer.Create(personId, "Strength", hireDate, "Initial notes");

        result.IsSuccess.Should().BeTrue();
        result.Value.PersonId.Should().Be(personId);
        result.Value.Specialization.Should().Be("Strength");
        result.Value.HireDate.Should().Be(hireDate);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Notes.Should().Be("Initial notes");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_HireDateInFuture_ReturnsValidationFailure()
    {
        var result = Trainer.Create(
            Id.Create(3).Value,
            "Cardio",
            DateTime.UtcNow.AddDays(1),
            null);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Trainer.HireDate.InTheFuture");
    }

    [Fact]
    public void Create_SpecializationTooLong_ReturnsValidationFailure()
    {
        var result = Trainer.Create(
            Id.Create(4).Value,
            new string('x', Trainer.Constants.SpecializationMaxLength + 1),
            DateTime.UtcNow.AddDays(-1),
            null);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Trainer.Specialization.TooLong");
    }

    [Fact]
    public void Update_ValidData_UpdatesAllowedFieldsAndSetsUpdatedAt()
    {
        var trainer = Trainer.Create(
            Id.Create(7).Value,
            "Old specialization",
            DateTime.UtcNow.AddDays(-30),
            "Old notes").Value;

        var createdAt = trainer.CreatedAt;

        var result = trainer.Update("New specialization", false, "New notes");

        result.IsSuccess.Should().BeTrue();
        trainer.Specialization.Should().Be("New specialization");
        trainer.IsActive.Should().BeFalse();
        trainer.Notes.Should().Be("New notes");
        trainer.CreatedAt.Should().Be(createdAt);
        trainer.UpdatedAt.Should().NotBeNull();
        trainer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_NotesTooLong_ReturnsValidationFailure()
    {
        var trainer = Trainer.Create(
            Id.Create(8).Value,
            "Functional",
            DateTime.UtcNow.AddDays(-5),
            "ok").Value;

        var result = trainer.Update("Functional", true, new string('x', Trainer.Constants.NotesMaxLength + 1));

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Trainer.Notes.TooLong");
    }
}

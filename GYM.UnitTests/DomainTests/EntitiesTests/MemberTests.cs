using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class MemberTests
{
    [Fact]
    public void Create_ValidData_ReturnsMemberWithExpectedState()
    {
        var personId = Id.Create(5).Value;
        var trainerId = Id.Create(7).Value;
        var joinDate = DateTime.UtcNow.Date.AddDays(-20);

        var result = Member.Create(
            personId,
            trainerId,
            "  Jane Doe  ",
            "  +1234567890  ",
            joinDate,
            "  New member notes  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.PersonId.Should().Be(personId);
        result.Value.TrainerId.Should().Be(trainerId);
        result.Value.EmergencyContactName.Should().Be("Jane Doe");
        result.Value.EmergencyContactPhone.Should().Be("+1234567890");
        result.Value.JoinDate.Should().Be(joinDate);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Notes.Should().Be("New member notes");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_JoinDateInFuture_ReturnsValidationFailure()
    {
        var result = Member.Create(
            Id.Create(3).Value,
            null,
            "Emergency",
            "+1111111111",
            DateTime.UtcNow.Date.AddDays(1),
            null);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Member.JoinDate.InTheFuture.Code);
    }

    [Fact]
    public void Create_EmergencyContactPhoneTooLong_ReturnsValidationFailure()
    {
        var result = Member.Create(
            Id.Create(4).Value,
            null,
            "Emergency",
            new string('1', Member.Constants.EmergencyContactPhoneMaxLength + 1),
            DateTime.UtcNow.Date,
            null);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Member.EmergencyContactPhone.TooLong.Code);
    }

    [Fact]
    public void Update_ValidData_UpdatesAllowedFieldsAndPreservesJoinDate()
    {
        var member = Member.Create(
            Id.Create(8).Value,
            Id.Create(9).Value,
            "Old Contact",
            "+2000000000",
            DateTime.UtcNow.Date.AddDays(-60),
            "Old notes").Value;

        var createdAt = member.CreatedAt;
        var joinDate = member.JoinDate;

        var result = member.Update(
            null,
            "  New Contact  ",
            "  +3000000000  ",
            false,
            "  Updated notes  ");

        result.IsSuccess.Should().BeTrue();
        member.TrainerId.Should().BeNull();
        member.EmergencyContactName.Should().Be("New Contact");
        member.EmergencyContactPhone.Should().Be("+3000000000");
        member.IsActive.Should().BeFalse();
        member.Notes.Should().Be("Updated notes");
        member.JoinDate.Should().Be(joinDate);
        member.CreatedAt.Should().Be(createdAt);
        member.UpdatedAt.Should().NotBeNull();
        member.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_NotesTooLong_ReturnsValidationFailure()
    {
        var member = Member.Create(
            Id.Create(10).Value,
            null,
            null,
            null,
            DateTime.UtcNow.Date.AddDays(-5),
            "ok").Value;

        var result = member.Update(
            null,
            null,
            null,
            true,
            new string('x', Member.Constants.NotesMaxLength + 1));

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Member.Notes.TooLong.Code);
    }
}

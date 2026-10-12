using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class PersonTests
{
    [Fact]
    public void Create_ValidData_ReturnsPersonWithExpectedValues()
    {
        var gymId = Id.Create(7).Value;
        var phone = Phone.Create("+1234567890").Value;
        var email = Email.Create("jane.doe@example.com").Value;
        var dateOfBirth = DateOfBirth.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-29))).Value;

        var result = Person.Create(
            gymId,
            "Jane Doe",
            phone,
            email,
            dateOfBirth,
            Gender.Female,
            "123 Main Street");

        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Jane Doe");
        result.Value.GymId.Value.Should().Be(7);
        result.Value.Phone.Value.Should().Be("+1234567890");
        result.Value.Email!.Value.Should().Be("jane.doe@example.com");
        result.Value.DateOfBirth!.Value.Should().Be(dateOfBirth.Value);
        result.Value.Gender.Should().Be(Gender.Female);
        result.Value.Address.Should().Be("123 Main Street");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_FullNameTooShort_ReturnsFailure()
    {
        var result = Person.Create(
            Id.Create(1).Value,
            "A",
            Phone.Create("+1234567890").Value,
            null,
            null,
            Gender.Male,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validationErrors = (ValidationErrors)result.Error;
        validationErrors.Errors.Should().ContainSingle(error => error.Code == "Person.FullName.TooShort");
    }

    [Fact]
    public void Create_AddressTooLong_ReturnsFailure()
    {
        var result = Person.Create(
            Id.Create(1).Value,
            "John Doe",
            Phone.Create("+1234567890").Value,
            null,
            null,
            Gender.Male,
            new string('x', 201));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validationErrors = (ValidationErrors)result.Error;
        validationErrors.Errors.Should().ContainSingle(error => error.Code == "Person.Address.TooLong");
    }

    [Fact]
    public void Update_ValidData_ChangesPropertiesAndSetsUpdatedAt()
    {
        var person = Person.Create(
            Id.Create(9).Value,
            "Old Name",
            Phone.Create("+1234567890").Value,
            null,
            null,
            Gender.Male,
            "Old address").Value;

        var updatedPhone = Phone.Create("+1987654321").Value;
        var updatedEmail = Email.Create("updated@example.com").Value;
        var updatedDob = DateOfBirth.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-35))).Value;

        var result = person.Update(
            "New Name",
            updatedPhone,
            updatedEmail,
            updatedDob,
            Gender.Female,
            "New address");

        result.IsSuccess.Should().BeTrue();
        person.FullName.Should().Be("New Name");
        person.Phone.Value.Should().Be("+1987654321");
        person.Email!.Value.Should().Be("updated@example.com");
        person.DateOfBirth!.Value.Should().Be(updatedDob.Value);
        person.Gender.Should().Be(Gender.Female);
        person.Address.Should().Be("New address");
        person.UpdatedAt.Should().NotBeNull();
        person.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

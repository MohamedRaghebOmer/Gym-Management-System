using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class UserTests
{
    [Fact]
    public void Create_ValidData_ReturnsUserWithExpectedState()
    {
        var personId = Id.Create(5).Value;
        var roleId = Id.Create(2).Value;

        var result = User.Create(personId, "john.doe", "hashed-password", roleId);

        result.IsSuccess.Should().BeTrue();
        result.Value.PersonId.Should().Be(personId);
        result.Value.RoleId.Should().Be(roleId);
        result.Value.Username.Should().Be("john.doe");
        result.Value.PasswordHash.Should().Be("hashed-password");
        result.Value.IsActive.Should().BeTrue();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_UsernameTooShort_ReturnsValidationFailure()
    {
        var personId = Id.Create(5).Value;
        var roleId = Id.Create(2).Value;

        var result = User.Create(personId, "a", "hashed-password", roleId);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "User.Username.TooShort");
    }

    [Fact]
    public void Update_ValidData_UpdatesAllowedFieldsAndSetsUpdatedAt()
    {
        var user = User.Create(
            Id.Create(5).Value,
            "old.user",
            "old-hash",
            Id.Create(2).Value).Value;

        var createdAt = user.CreatedAt;

        var result = user.Update("new.user", "new-hash", false);

        result.IsSuccess.Should().BeTrue();
        user.Username.Should().Be("new.user");
        user.PasswordHash.Should().Be("new-hash");
        user.IsActive.Should().BeFalse();
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_EmptyPasswordHash_ReturnsValidationFailure()
    {
        var user = User.Create(
            Id.Create(5).Value,
            "old.user",
            "old-hash",
            Id.Create(2).Value).Value;

        var result = user.Update("new.user", string.Empty, true);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "User.Password.Empty");
    }
}

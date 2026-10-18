using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Shared;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class RoleTests
{
    [Fact]
    public void Create_ValidInputs_Succeeds()
    {
        var result = Role.Create("Administrator", "Has full access");

        result.IsSuccess.Should().BeTrue();
        var role = result.Value;
        role.Name.Should().Be("Administrator");
        role.Description.Should().Be("Has full access");
        role.CreatedAt.Should().BeCloseTo(System.DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_NameTooShort_ReturnsValidationFailure()
    {
        var result = Role.Create("A", "desc");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validation = result.Error.As<ValidationErrors>();
        validation.Errors.Should().Contain(e => e.Code == "Role.Name.TooShort");
    }

    [Fact]
    public void Create_DescriptionTooLong_ReturnsValidationFailure()
    {
        var longDescription = new string('x', Role.Constants.DescriptionMaxLength + 1);

        var result = Role.Create("Manager", longDescription);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validation = result.Error.As<ValidationErrors>();
        validation.Errors.Should().Contain(e => e.Code == "Role.Description.TooLong");
    }

    [Fact]
    public void Update_ValidValues_UpdatesStateAndSetsUpdatedAt()
    {
        var create = Role.Create("Member", "Basic member");
        create.IsSuccess.Should().BeTrue();
        var role = create.Value;

        var before = role.CreatedAt;
        var updateResult = role.Update("Premium Member", "Upgraded");

        updateResult.IsSuccess.Should().BeTrue();
        role.Name.Should().Be("Premium Member");
        role.Description.Should().Be("Upgraded");
        role.UpdatedAt.Should().NotBeNull();
        role.UpdatedAt.Should().BeAfter(before);
    }
}

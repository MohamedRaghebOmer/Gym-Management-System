using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class GymTests
{
    [Fact]
    public void Create_ValidName_ReturnsGym()
    {
        var result = Gym.Create("  My Gym  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Gym");
        result.Value.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Create_EmptyName_ReturnsValidationFailure()
    {
        var result = Gym.Create("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validation = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validation.Errors.Should().ContainSingle(e => e.Code == DomainErrors.Entities.Gym.Name.Empty.Code);
    }

    [Fact]
    public void Create_NameTooShort_ReturnsValidationFailure()
    {
        var result = Gym.Create("A");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validation = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validation.Errors.Should().ContainSingle(e => e.Code == DomainErrors.Entities.Gym.Name.TooShort.Code);
    }

    [Fact]
    public void Update_ValidName_UpdatesNameAndUpdatedAt()
    {
        var gym = Gym.Create("Original").Value;

        var result = gym.Update("Updated");

        result.IsSuccess.Should().BeTrue();
        gym.Name.Should().Be("Updated");
        gym.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_InvalidName_ReturnsValidationFailureAndDoesNotChangeName()
    {
        var gym = Gym.Create("Original").Value;

        var result = gym.Update(" ");

        result.IsFailure.Should().BeTrue();
        gym.Name.Should().Be("Original");
    }
}

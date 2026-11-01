using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class MembershipPlanTests
{
    [Fact]
    public void Create_ValidInput_ReturnsMembershipPlan()
    {
        var gymId = Id.Create(1).Value;
        var price = Money.Create(199.99m).Value;

        var result = MembershipPlan.Create(gymId, "  Gold  ", 30, price, "  Full access  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.GymId.Should().Be(gymId);
        result.Value.Name.Should().Be("Gold");
        result.Value.DurationInDays.Should().Be(30);
        result.Value.Price.Should().Be(price);
        result.Value.Description.Should().Be("Full access");
        result.Value.IsActive.Should().BeTrue();
        result.Value.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Create_EmptyName_ReturnsValidationFailure()
    {
        var result = MembershipPlan.Create(
            Id.Create(1).Value,
            "   ",
            30,
            Money.Create(100m).Value,
            "desc");

        result.IsFailure.Should().BeTrue();
        var validation = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validation.Errors.Should().ContainSingle(e => e.Code == DomainErrors.Entities.MembershipPlan.Name.Empty.Code);
    }

    [Fact]
    public void Create_NonPositiveDuration_ReturnsValidationFailure()
    {
        var result = MembershipPlan.Create(
            Id.Create(1).Value,
            "Basic",
            0,
            Money.Create(100m).Value,
            "desc");

        result.IsFailure.Should().BeTrue();
        var validation = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validation.Errors.Should().ContainSingle(e => e.Code == DomainErrors.Entities.MembershipPlan.DurationInDays.LessThanOrEqualToZero.Code);
    }

    [Fact]
    public void Update_ValidInput_UpdatesEntityAndUpdatedAt()
    {
        var plan = MembershipPlan.Create(
            Id.Create(1).Value,
            "Basic",
            30,
            Money.Create(100m).Value,
            "desc").Value;

        var updatedPrice = Money.Create(150m).Value;

        var result = plan.Update("  Premium  ", 60, updatedPrice, "  New desc  ", false);

        result.IsSuccess.Should().BeTrue();
        plan.Name.Should().Be("Premium");
        plan.DurationInDays.Should().Be(60);
        plan.Price.Should().Be(updatedPrice);
        plan.Description.Should().Be("New desc");
        plan.IsActive.Should().BeFalse();
        plan.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_InvalidInput_ReturnsValidationFailureAndDoesNotChangeEntity()
    {
        var plan = MembershipPlan.Create(
            Id.Create(1).Value,
            "Basic",
            30,
            Money.Create(100m).Value,
            "desc").Value;

        var originalName = plan.Name;
        var originalDuration = plan.DurationInDays;
        var originalPrice = plan.Price;

        var result = plan.Update("", 0, Money.Create(120m).Value, new string('a', MembershipPlan.Constants.DescriptionMaxLength + 1), true);

        result.IsFailure.Should().BeTrue();
        plan.Name.Should().Be(originalName);
        plan.DurationInDays.Should().Be(originalDuration);
        plan.Price.Should().Be(originalPrice);
    }
}

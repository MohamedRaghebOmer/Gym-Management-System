using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class SubscriptionTests
{
    [Fact]
    public void Create_ValidInput_ReturnsSubscriptionWithExpectedState()
    {
        var memberId = Id.Create(5).Value;
        var membershipPlanId = Id.Create(9).Value;
        var price = Money.Create(299.99m).Value;
        var startDate = new DateOnly(2026, 1, 1);
        var endDate = new DateOnly(2026, 2, 1);

        var result = Subscription.Create(memberId, membershipPlanId, startDate, endDate, price);

        result.IsSuccess.Should().BeTrue();
        result.Value.MemberId.Should().Be(memberId);
        result.Value.MembershipPlanId.Should().Be(membershipPlanId);
        result.Value.StartDate.Should().Be(startDate);
        result.Value.EndDate.Should().Be(endDate);
        result.Value.Price.Should().Be(price);
        result.Value.Status.Should().Be(SubscriptionStatus.Active);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_DefaultStartDate_ReturnsValidationFailure()
    {
        var result = Subscription.Create(
            Id.Create(1).Value,
            Id.Create(2).Value,
            default,
            new DateOnly(2026, 1, 2),
            Money.Create(100m).Value);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Subscription.StartDate.Required.Code);
    }

    [Fact]
    public void Create_EndDateLessThanOrEqualToStartDate_ReturnsValidationFailure()
    {
        var startDate = new DateOnly(2026, 1, 5);

        var result = Subscription.Create(
            Id.Create(1).Value,
            Id.Create(2).Value,
            startDate,
            startDate,
            Money.Create(100m).Value);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Subscription.EndDate.MustBeGreaterThanStartDate.Code);
    }

    [Fact]
    public void Update_ValidInput_UpdatesEntityAndUpdatedAt()
    {
        var subscription = Subscription.Create(
            Id.Create(3).Value,
            Id.Create(7).Value,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 1),
            Money.Create(150m).Value).Value;

        var newPrice = Money.Create(180m).Value;
        var newStartDate = new DateOnly(2026, 5, 1);
        var newEndDate = new DateOnly(2026, 6, 1);

        var result = subscription.Update(
            newStartDate,
            newEndDate,
            newPrice,
            SubscriptionStatus.Cancelled);

        result.IsSuccess.Should().BeTrue();
        subscription.StartDate.Should().Be(newStartDate);
        subscription.EndDate.Should().Be(newEndDate);
        subscription.Price.Should().Be(newPrice);
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.UpdatedAt.Should().NotBeNull();
        subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_InvalidDates_ReturnsValidationFailureWithoutChangingState()
    {
        var subscription = Subscription.Create(
            Id.Create(3).Value,
            Id.Create(7).Value,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 8, 1),
            Money.Create(170m).Value).Value;

        var originalStartDate = subscription.StartDate;
        var originalEndDate = subscription.EndDate;
        var originalPrice = subscription.Price;
        var originalStatus = subscription.Status;

        var result = subscription.Update(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 1),
            Money.Create(220m).Value,
            SubscriptionStatus.Expired);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Subscription.EndDate.MustBeGreaterThanStartDate.Code);

        subscription.StartDate.Should().Be(originalStartDate);
        subscription.EndDate.Should().Be(originalEndDate);
        subscription.Price.Should().Be(originalPrice);
        subscription.Status.Should().Be(originalStatus);
    }
}

using GYM.Domain.Enums;
using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Subscription : Entity
{
    private Subscription(
        Id memberId,
        Id membershipPlanId,
        DateOnly startDate,
        DateOnly endDate,
        Money price)
    {
        MemberId = memberId;
        MembershipPlanId = membershipPlanId;
        StartDate = startDate;
        EndDate = endDate;
        Price = price;
        Status = SubscriptionStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    private Subscription() { } // For EF Core

    public Id MemberId { get; private set; } = default!;
    public Id MembershipPlanId { get; private set; } = default!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Money Price { get; private set; } = default!;
    public SubscriptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public Member? Member { get; private set; }
    public MembershipPlan? MembershipPlan { get; private set; }

    public static Result<Subscription> Create(
        Id memberId,
        Id membershipPlanId,
        DateOnly startDate,
        DateOnly endDate,
        Money price)
    {
        var result = Validate(startDate, endDate);
        if (result.IsFailure)
            return Result.Failure<Subscription>(result.Error);

        return new Subscription(memberId, membershipPlanId, startDate, endDate, price);
    }

    public Result Update(
        DateOnly startDate,
        DateOnly endDate,
        Money price,
        SubscriptionStatus status)
    {
        var result = Validate(startDate, endDate);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        StartDate = startDate;
        EndDate = endDate;
        Price = price;
        Status = status;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        DateOnly startDate,
        DateOnly endDate)
    {
        List<Error> errors = [];

        if (startDate == default)
            errors.Add(DomainErrors.Entities.Subscription.StartDate.Required);

        if (endDate == default)
            errors.Add(DomainErrors.Entities.Subscription.EndDate.Required);

        if (startDate != default && endDate != default && endDate <= startDate)
            errors.Add(DomainErrors.Entities.Subscription.EndDate.MustBeGreaterThanStartDate);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

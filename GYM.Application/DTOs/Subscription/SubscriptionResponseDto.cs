using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Subscription;

public sealed record SubscriptionResponseDto(
    int Id,
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Price,
    Domain.Enums.SubscriptionStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    : IResponseDto<Domain.Entities.Subscription, SubscriptionResponseDto>
{
    public static SubscriptionResponseDto FromEntity(Domain.Entities.Subscription entity)
    {
        return new SubscriptionResponseDto(
            Id: entity.Id.Value,
            MemberId: entity.MemberId.Value,
            MembershipPlanId: entity.MembershipPlanId.Value,
            StartDate: entity.StartDate,
            EndDate: entity.EndDate,
            Price: entity.Price.Value,
            Status: entity.Status,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);
    }
}

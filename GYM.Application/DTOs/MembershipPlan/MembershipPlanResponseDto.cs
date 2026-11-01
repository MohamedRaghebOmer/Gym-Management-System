using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.MembershipPlan;

public sealed record MembershipPlanResponseDto(
    int Id,
    int GymId,
    string Name,
    int DurationInDays,
    decimal Price,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    : IResponseDto<Domain.Entities.MembershipPlan, MembershipPlanResponseDto>
{
    public static MembershipPlanResponseDto FromEntity(Domain.Entities.MembershipPlan entity)
    {
        return new MembershipPlanResponseDto(
            Id: entity.Id.Value,
            GymId: entity.GymId.Value,
            Name: entity.Name,
            DurationInDays: entity.DurationInDays,
            Price: entity.Price.Value,
            Description: entity.Description,
            IsActive: entity.IsActive,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);
    }
}

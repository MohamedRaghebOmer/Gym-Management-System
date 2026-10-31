using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Member;

public sealed record MemberResponseDto(
    int Id,
    int PersonId,
    int? TrainerId,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    DateTime JoinDate,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    : IResponseDto<Domain.Entities.Member, MemberResponseDto>
{
    public static MemberResponseDto FromEntity(Domain.Entities.Member entity)
    {
        return new MemberResponseDto(
            Id: entity.Id.Value,
            PersonId: entity.PersonId.Value,
            TrainerId: entity.TrainerId?.Value,
            EmergencyContactName: entity.EmergencyContactName,
            EmergencyContactPhone: entity.EmergencyContactPhone,
            JoinDate: entity.JoinDate,
            IsActive: entity.IsActive,
            Notes: entity.Notes,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);
    }
}

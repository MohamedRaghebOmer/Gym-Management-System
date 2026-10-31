using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Trainer;

public sealed record TrainerResponseDto(
    int Id,
    int PersonId,
    string? Specialization,
    DateTime HireDate,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    : IResponseDto<Domain.Entities.Trainer, TrainerResponseDto>
{
    public static TrainerResponseDto FromEntity(Domain.Entities.Trainer entity)
    {
        return new TrainerResponseDto(
            Id: entity.Id.Value,
            PersonId: entity.PersonId.Value,
            Specialization: entity.Specialization,
            HireDate: entity.HireDate,
            IsActive: entity.IsActive,
            Notes: entity.Notes,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);
    }
}

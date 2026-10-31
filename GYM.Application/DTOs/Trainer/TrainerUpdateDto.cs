using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Trainer;

public sealed record TrainerUpdateDto : IUpdateDto
{
    [MaxLength(Domain.Entities.Trainer.Constants.SpecializationMaxLength, ErrorMessage = "Specialization is too long.")]
    public string? Specialization { get; init; }

    [Required]
    public required bool IsActive { get; init; }

    [MaxLength(Domain.Entities.Trainer.Constants.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; init; }
}

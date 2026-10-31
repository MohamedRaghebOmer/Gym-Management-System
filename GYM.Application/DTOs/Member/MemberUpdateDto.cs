using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Member;

public sealed record MemberUpdateDto : IUpdateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "TrainerId must be a positive integer when provided.")]
    public int? TrainerId { get; init; }

    [MaxLength(Domain.Entities.Member.Constants.EmergencyContactNameMaxLength, ErrorMessage = "Emergency contact name is too long.")]
    public string? EmergencyContactName { get; init; }

    [MaxLength(Domain.Entities.Member.Constants.EmergencyContactPhoneMaxLength, ErrorMessage = "Emergency contact phone is too long.")]
    public string? EmergencyContactPhone { get; init; }

    [Required]
    public required bool IsActive { get; init; }

    [MaxLength(Domain.Entities.Member.Constants.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; init; }
}

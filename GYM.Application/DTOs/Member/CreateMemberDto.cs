using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Member;

public sealed record CreateMemberDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PersonId must be a positive integer.")]
    public required int PersonId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "TrainerId must be a positive integer when provided.")]
    public int? TrainerId { get; init; }

    [MaxLength(Domain.Entities.Member.Constants.EmergencyContactNameMaxLength, ErrorMessage = "Emergency contact name is too long.")]
    public string? EmergencyContactName { get; init; }

    [MaxLength(Domain.Entities.Member.Constants.EmergencyContactPhoneMaxLength, ErrorMessage = "Emergency contact phone is too long.")]
    public string? EmergencyContactPhone { get; init; }

    [Required]
    [DataType(DataType.Date, ErrorMessage = "JoinDate must be a valid date.")]
    public required DateTime JoinDate { get; init; }

    [MaxLength(Domain.Entities.Member.Constants.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; init; }
}

using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Trainer;

public sealed record CreateTrainerDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PersonId must be a positive integer.")]
    public required int PersonId { get; init; }


    [MaxLength(Domain.Entities.Trainer.Constants.SpecializationMaxLength, ErrorMessage = "Specialization is too long.")]
    public string? Specialization { get; init; }


    [Required]
    [DataType(DataType.Date, ErrorMessage = "HireDate must be a valid date.")]
    public required DateTime HireDate { get; init; }


    [MaxLength(Domain.Entities.Trainer.Constants.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; init; }
}

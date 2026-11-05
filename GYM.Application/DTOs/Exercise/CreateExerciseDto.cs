using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Exercise;

public sealed record CreateExerciseDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "GymId must be a positive integer.")]
    public required int GymId { get; init; }

    [Required]
    [MaxLength(Domain.Entities.Exercise.Constants.NameMaxLength, ErrorMessage = "Name is too long.")]
    public required string Name { get; init; }

    [Required]
    [MaxLength(Domain.Entities.Exercise.Constants.MuscleGroupMaxLength, ErrorMessage = "MuscleGroup is too long.")]
    public required string MuscleGroup { get; init; }

    [MaxLength(Domain.Entities.Exercise.Constants.DescriptionMaxLength, ErrorMessage = "Description is too long.")]
    public string? Description { get; init; }

    [MaxLength(Domain.Entities.Exercise.Constants.InstructionsMaxLength, ErrorMessage = "Instructions is too long.")]
    public string? Instructions { get; init; }

    [MaxLength(Domain.Entities.Exercise.Constants.VideoUrlMaxLength, ErrorMessage = "VideoUrl is too long.")]
    public string? VideoUrl { get; init; }
}

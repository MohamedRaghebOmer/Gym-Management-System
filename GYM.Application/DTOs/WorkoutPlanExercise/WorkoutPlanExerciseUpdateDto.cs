using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.WorkoutPlanExercise;

public sealed record WorkoutPlanExerciseUpdateDto : IUpdateDto
{
    [Required]
    [Range(1, byte.MaxValue, ErrorMessage = "Sets must be greater than zero.")]
    public required byte Sets { get; init; }

    [Range(1, byte.MaxValue, ErrorMessage = "Reps must be greater than zero.")]
    public byte? Reps { get; init; }

    [Range(0, 9999.99, ErrorMessage = "Weight must be between 0 and 9999.99.")]
    public decimal? Weight { get; init; }

    [Range(0, short.MaxValue, ErrorMessage = "RestSeconds must be greater than or equal to zero.")]
    public short? RestSeconds { get; init; }

    [Required]
    public required byte DisplayOrder { get; init; }

    [MaxLength(Domain.Entities.WorkoutPlanExercise.Constants.NotesMaxLength, ErrorMessage = "Notes is too long.")]
    public string? Notes { get; init; }
}

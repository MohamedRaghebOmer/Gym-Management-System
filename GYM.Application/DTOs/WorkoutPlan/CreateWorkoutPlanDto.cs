using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.WorkoutPlan;

public sealed record CreateWorkoutPlanDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive integer.")]
    public int MemberId { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "TrainerId must be a positive integer.")]
    public int TrainerId { get; init; }

    [Required]
    [MaxLength(Domain.Entities.WorkoutPlan.Constants.NameMaxLength,
        ErrorMessage = "Name is too long.")]
    public string Name { get; init; } = default!;

    [MaxLength(Domain.Entities.WorkoutPlan.Constants.DescriptionMaxLength,
        ErrorMessage = "Description is too long.")]
    public string? Description { get; init; }

    [Required]
    [DataType(DataType.Date, ErrorMessage = "StartDate must be a valid date.")]
    public DateOnly StartDate { get; init; }

    [DataType(DataType.Date, ErrorMessage = "EndDate must be a valid date.")]
    public DateOnly? EndDate { get; init; }
}
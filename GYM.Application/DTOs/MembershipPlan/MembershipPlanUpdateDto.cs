using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.MembershipPlan;

public sealed record MembershipPlanUpdateDto : IUpdateDto
{
    [Required]
    [MaxLength(Domain.Entities.MembershipPlan.Constants.NameMaxLength, ErrorMessage = "Name is too long.")]
    public required string Name { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DurationInDays must be greater than zero.")]
    public required int DurationInDays { get; init; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be greater than zero.")]
    public required decimal Price { get; init; }

    [MaxLength(Domain.Entities.MembershipPlan.Constants.DescriptionMaxLength, ErrorMessage = "Description is too long.")]
    public string? Description { get; init; }

    [Required]
    public required bool IsActive { get; init; }
}

using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Subscription;

public sealed record CreateSubscriptionDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive integer.")]
    public required int MemberId { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MembershipPlanId must be a positive integer.")]
    public required int MembershipPlanId { get; init; }

    [Required]
    [DataType(DataType.Date, ErrorMessage = "StartDate must be a valid date.")]
    public required DateOnly StartDate { get; init; }

    [Required]
    [DataType(DataType.Date, ErrorMessage = "EndDate must be a valid date.")]
    public required DateOnly EndDate { get; init; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be greater than zero.")]
    public required decimal Price { get; init; }
}

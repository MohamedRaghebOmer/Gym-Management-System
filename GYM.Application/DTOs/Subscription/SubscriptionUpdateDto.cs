using GYM.Application.Abstractions;
using GYM.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Subscription;

public sealed record SubscriptionUpdateDto : IUpdateDto
{
    [Required]
    [DataType(DataType.Date, ErrorMessage = "StartDate must be a valid date.")]
    public required DateOnly StartDate { get; init; }

    [Required]
    [DataType(DataType.Date, ErrorMessage = "EndDate must be a valid date.")]
    public required DateOnly EndDate { get; init; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be greater than zero.")]
    public required decimal Price { get; init; }

    [Required]
    [EnumDataType(typeof(SubscriptionStatus))]
    public required SubscriptionStatus Status { get; init; }
}

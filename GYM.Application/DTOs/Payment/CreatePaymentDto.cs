using GYM.Application.Abstractions;
using GYM.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Payment;

public sealed record CreatePaymentDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "SubscriptionId must be a positive integer.")]
    public required int SubscriptionId { get; init; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero.")]
    public required decimal Amount { get; init; }

    [Required]
    [EnumDataType(typeof(PaymentMethod))]
    public required PaymentMethod PaymentMethod { get; init; }

    [Required]
    [DataType(DataType.DateTime, ErrorMessage = "PaymentDate must be a valid date time.")]
    public required DateTime PaymentDate { get; init; }

    [MaxLength(Domain.Entities.Payment.Constants.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; init; }
}

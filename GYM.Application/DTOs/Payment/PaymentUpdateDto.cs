using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Payment;

public sealed record PaymentUpdateDto : IUpdateDto
{
    [MaxLength(Domain.Entities.Payment.Constants.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; init; }
}

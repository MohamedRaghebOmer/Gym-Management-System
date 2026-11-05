using GYM.Application.Abstractions;
using GYM.Domain.Enums;

namespace GYM.Application.DTOs.Payment;

public sealed record PaymentResponseDto(
    int Id,
    int SubscriptionId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    DateTime PaymentDate,
    string? Notes,
    DateTime CreatedAt)
    : IResponseDto<Domain.Entities.Payment, PaymentResponseDto>
{
    public static PaymentResponseDto FromEntity(Domain.Entities.Payment entity)
    {
        return new PaymentResponseDto(
            Id: entity.Id.Value,
            SubscriptionId: entity.SubscriptionId.Value,
            Amount: entity.Amount.Value,
            PaymentMethod: entity.PaymentMethod,
            PaymentDate: entity.PaymentDate,
            Notes: entity.Notes,
            CreatedAt: entity.CreatedAt);
    }
}

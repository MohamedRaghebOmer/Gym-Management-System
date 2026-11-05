using GYM.Application.DTOs.Payment;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IPaymentService :
    IService<Payment, PaymentResponseDto, CreatePaymentDto, PaymentUpdateDto>
{
}

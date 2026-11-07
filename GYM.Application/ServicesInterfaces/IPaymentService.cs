using GYM.Application.Abstractions;
using GYM.Application.DTOs.Payment;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IPaymentService :
    IService<Payment, PaymentResponseDto, CreatePaymentDto, PaymentUpdateDto>
{
}

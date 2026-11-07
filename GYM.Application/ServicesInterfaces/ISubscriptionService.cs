using GYM.Application.Abstractions;
using GYM.Application.DTOs.Subscription;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface ISubscriptionService :
    IService<Subscription, SubscriptionResponseDto, CreateSubscriptionDto, SubscriptionUpdateDto>
{
}

using GYM.Application.DTOs.Subscription;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface ISubscriptionService :
    IService<Subscription, SubscriptionResponseDto, CreateSubscriptionDto, SubscriptionUpdateDto>
{
}

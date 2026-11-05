using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<bool> HasOverlappingActiveSubscriptionAsync(
        Id memberId,
        DateOnly startDate,
        DateOnly endDate,
        Id? excludedSubscriptionId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveSubscriptionAtAsync(
        Id memberId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}

using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Repositories;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    private readonly GymDbContext _dbContext;

    public SubscriptionRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasOverlappingActiveSubscriptionAsync(
        Id memberId,
        DateOnly startDate,
        DateOnly endDate,
        Id? excludedSubscriptionId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Subscription> query = _dbContext.Set<Subscription>()
            .Where(s =>
                s.MemberId == memberId
                && s.Status == SubscriptionStatus.Active
                && s.StartDate <= endDate
                && s.EndDate >= startDate);

        if (excludedSubscriptionId is not null)
        {
            query = query.Where(s => s.Id != excludedSubscriptionId);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> HasActiveSubscriptionAtAsync(
        Id memberId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Subscription>()
            .AnyAsync(
                s => s.MemberId == memberId
                    && s.Status == SubscriptionStatus.Active
                    && s.StartDate <= date
                    && s.EndDate >= date,
                cancellationToken);
    }
}

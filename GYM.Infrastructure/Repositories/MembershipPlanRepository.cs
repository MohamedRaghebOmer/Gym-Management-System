using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class MembershipPlanRepository : Repository<MembershipPlan>, IMembershipPlanRepository
{
    private readonly GymDbContext _dbContext;

    public MembershipPlanRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MembershipPlan?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<MembershipPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(mp => mp.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<MembershipPlan>()
            .AnyAsync(mp => mp.Name == name, cancellationToken);
    }
}

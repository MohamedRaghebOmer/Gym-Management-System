using GYM.Domain.Abstractions;
using GYM.Infrastructure.Persistence;

namespace GYM.Infrastructure;

public sealed class UnitOfWork(GymDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
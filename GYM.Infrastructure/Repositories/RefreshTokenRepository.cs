using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    private readonly GymDbContext _dbContext;

    public RefreshTokenRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public Task<bool> ExistsByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<RefreshToken>()
            .AnyAsync(rt => rt.Token == token, cancellationToken);
    }
}

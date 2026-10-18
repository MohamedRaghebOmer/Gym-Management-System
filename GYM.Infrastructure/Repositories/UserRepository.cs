using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    private readonly GymDbContext _dbContext;

    public UserRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByPersonIdAsync(GYM.Domain.ValueObjects.Id personId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PersonId == personId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public Task<bool> ExistsByPersonIdAsync(GYM.Domain.ValueObjects.Id personId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<User>()
            .AnyAsync(u => u.PersonId == personId, cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<User>()
            .AnyAsync(u => u.Username == username, cancellationToken);
    }
}

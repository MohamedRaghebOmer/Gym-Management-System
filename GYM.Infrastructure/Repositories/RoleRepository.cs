using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;

namespace GYM.Infrastructure.Repositories;

public sealed class RoleRepository : Repository<Role>, IRoleRepository
{
    private readonly GymDbContext _dbContext;

    public RoleRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

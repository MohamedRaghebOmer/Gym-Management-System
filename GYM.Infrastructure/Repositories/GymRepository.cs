using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;

namespace GYM.Infrastructure.Repositories;

internal sealed class GymRepository : Repository<Gym>, IGymRepository
{
    private readonly GymDbContext _dbContext;

    public GymRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

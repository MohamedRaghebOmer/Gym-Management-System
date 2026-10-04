using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;

namespace GYM.Infrastructure.Repositories;

public sealed class PersonRepository : Repository<Person>, IPersonRepository
{
    private readonly GymDbContext _dbContext;

    public PersonRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }
}
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Repositories;

public interface ITrainerRepository : IRepository<Trainer>
{
    Task<Trainer?> GetByPersonIdAsync(
        Id personId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByPersonIdAsync(
        Id personId,
        CancellationToken cancellationToken = default);
}

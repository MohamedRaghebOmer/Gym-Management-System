using GYM.Domain.Abstractions;
using GYM.Domain.Entities;

namespace GYM.Domain.Repositories;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<Exercise?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}

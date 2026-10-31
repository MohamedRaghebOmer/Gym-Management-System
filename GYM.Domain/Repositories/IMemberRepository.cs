using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    Task<Member?> GetByPersonIdAsync(
        Id personId,
        CancellationToken cancellationToken = default);

    Task<Member?> GetByTrainerIdAsync(
        Id trainerId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByPersonIdAsync(
        Id personId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByTrainerIdAsync(
        Id trainerId,
        CancellationToken cancellationToken = default);
}

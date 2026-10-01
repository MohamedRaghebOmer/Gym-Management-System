using GYM.Domain.Primitives;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Abstractions;

public interface IRepository<TEntity>
    where TEntity : Entity
{
    void Add(TEntity entity);

    Task<bool> ExistsAsync(
        Id id,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(
        Id id,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        Id id,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
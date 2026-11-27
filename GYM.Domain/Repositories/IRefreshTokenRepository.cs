using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);
}

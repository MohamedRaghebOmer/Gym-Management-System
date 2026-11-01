using GYM.Domain.Abstractions;
using GYM.Domain.Entities;

namespace GYM.Domain.Repositories;

public interface IMembershipPlanRepository : IRepository<MembershipPlan>
{
    Task<MembershipPlan?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}

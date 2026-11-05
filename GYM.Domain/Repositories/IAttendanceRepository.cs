using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Repositories;

public interface IAttendanceRepository : IRepository<Attendance>
{
    Task<bool> HasOpenAttendanceAsync(
        Id memberId,
        Id? excludedAttendanceId = null,
        CancellationToken cancellationToken = default);
}

using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    private readonly GymDbContext _dbContext;

    public AttendanceRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasOpenAttendanceAsync(
        Id memberId,
        Id? excludedAttendanceId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Attendance> query = _dbContext.Set<Attendance>()
            .Where(a => a.MemberId == memberId && a.CheckOut == null);

        if (excludedAttendanceId is not null)
        {
            query = query.Where(a => a.Id != excludedAttendanceId);
        }

        return query.AnyAsync(cancellationToken);
    }
}

using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;

namespace GYM.Infrastructure.Repositories;

internal sealed class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(GymDbContext dbContext)
        : base(dbContext)
    {
    }
}

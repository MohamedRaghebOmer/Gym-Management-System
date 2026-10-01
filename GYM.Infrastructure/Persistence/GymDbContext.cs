using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Persistence;

public class GymDbContext(
    DbContextOptions<GymDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }
}
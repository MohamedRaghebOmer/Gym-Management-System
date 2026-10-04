using GYM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Persistence;

public sealed class GymDbContext(
    DbContextOptions<GymDbContext> options)
    : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }
}
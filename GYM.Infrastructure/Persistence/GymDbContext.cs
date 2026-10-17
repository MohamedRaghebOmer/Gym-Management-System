using GYM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Persistence;

public sealed class GymDbContext(
    DbContextOptions<GymDbContext> options)
    : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Gym> Gyms => Set<Gym>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }
}

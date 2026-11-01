using GYM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Persistence;

public sealed class GymDbContext(
    DbContextOptions<GymDbContext> options)
    : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }
}

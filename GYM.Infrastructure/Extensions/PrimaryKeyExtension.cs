using GYM.Domain.Primitives;
using GYM.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.Extensions;

public static class PrimaryKeyExtension
{
    public static EntityTypeBuilder<TEntity> ConfigurePrimaryKey<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => Id.FromDatabase(value))
            .UseIdentityAlwaysColumn();

        builder.HasKey(x => x.Id);

        return builder;
    }
}
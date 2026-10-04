using GYM.Domain.Primitives;
using GYM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GYM.Infrastructure.Abstractions;

public abstract class BaseEntityConfiguration<TEntity>
    where TEntity : Entity
{
    protected void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ConfigureTable(builder);
        ConfigureKey(builder);
        ConfigureProperties(builder);
    }

    protected static void ConfigureTable(EntityTypeBuilder<TEntity> builder) =>
        builder.ToTable(string.Concat(typeof(TEntity).Name, 's'));

    protected static void ConfigureKey(EntityTypeBuilder<TEntity> builder) =>
        builder.ConfigurePrimaryKey();

    protected abstract void ConfigureProperties(EntityTypeBuilder<TEntity> builder);
}
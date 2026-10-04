using GYM.Domain.Primitives;

namespace GYM.Application.Abstractions;

public interface IResponseDto<in TEntity, out TSelf>
    where TEntity : Entity
    where TSelf : IResponseDto<TEntity, TSelf>
{
    static abstract TSelf FromEntity(TEntity entity);
}
using GYM.Domain.Primitives;

namespace GYM.Application.Abstractions;

public interface IResponseDto<TEntity, TSelf>
    where TEntity : Entity
    where TSelf : IResponseDto<TEntity, TSelf>
{
    static abstract TSelf ToResponseDto(TEntity entity);
}
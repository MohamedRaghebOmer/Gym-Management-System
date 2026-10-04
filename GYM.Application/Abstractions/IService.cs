using GYM.Domain.Primitives;
using GYM.Domain.Shared;

namespace GYM.Application.Abstractions;

public interface IService<TEntity, TResponseDto, in TCreateDto, in TUpdateDto>
    where TEntity : Entity
    where TResponseDto : IResponseDto<TEntity, TResponseDto>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto
{
    Task<Result<int>> CreateAsync(
        TCreateDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<TResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<TResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<bool>> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(
        int id,
        TUpdateDto dto,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
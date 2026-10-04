using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Abstractions;

public abstract class ServiceBase<TEntity, TResponseDto, TCreateDto, TUpdateDto>
    : IService<TEntity, TResponseDto, TCreateDto, TUpdateDto>
    where TEntity : Entity
    where TResponseDto : IResponseDto<TEntity, TResponseDto>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto
{
    private readonly IRepository<TEntity> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ServiceBase<TEntity, TResponseDto, TCreateDto, TUpdateDto>> _logger;

    protected ServiceBase(
        IRepository<TEntity> repo,
        IUnitOfWork unitOfWork,
        ILogger<ServiceBase<TEntity, TResponseDto, TCreateDto, TUpdateDto>> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TResponseDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var idResult = Id.Create(id);

        if (idResult.IsFailure)
        {
            _logger.LogWarning(
                "Attempted to retrieve an entity of type {EntityType} with an invalid ID. {Id} {Error}",
                typeof(TEntity).Name,
                id,
                idResult.Error);

            return Result.Failure<TResponseDto>(idResult.Error);
        }

        var entity = await _repo.GetByIdAsync(
            idResult.Value,
            cancellationToken);

        if (entity is null)
        {
            return Result.Failure<TResponseDto>(
                ServiceErrors.Common.NotFound);
        }

        return TResponseDto.FromEntity(entity);
    }

    public async Task<List<TResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _repo.GetAllAsync(cancellationToken);

        return entities
            .Select(TResponseDto.FromEntity)
            .ToList();
    }

    public async Task<Result<bool>> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var idResult = Id.Create(id);

        if (idResult.IsFailure)
        {
            _logger.LogWarning(
                "Attempted to check the existence of entity of type {EntityType} with invalid ID. {Id} {Error}",
                typeof(TEntity).Name,
                id,
                idResult.Error);

            return Result.Failure<bool>(idResult.Error);
        }

        return await _repo.ExistsAsync(
            idResult.Value,
            cancellationToken);
    }

    public async Task<Result> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var idResult = Id.Create(id);

        if (idResult.IsFailure)
        {
            _logger.LogWarning(
                "Attempted to delete an entity of type {EntityType} with invalid ID. {Id} {Error}",
                typeof(TEntity).Name,
                id,
                idResult.Error);

            return Result.Failure(
                DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);
        }

        var isDeleted = await _repo.RemoveAsync(
            idResult.Value,
            cancellationToken);

        if (!isDeleted)
        {
            return Result.Failure<bool>(
                ServiceErrors.Common.NotFound);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public abstract Task<Result<int>> CreateAsync(
        TCreateDto dto,
        CancellationToken cancellationToken = default);

    public abstract Task<Result> UpdateAsync(
        int id,
        TUpdateDto dto,
        CancellationToken cancellationToken = default);
}
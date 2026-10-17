using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Gym;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class GymService :
    ServiceBase<Gym, GymResponseDto, CreateGymDto, UpdateGymDto>,
    IGymService
{
    private readonly IGymRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GymService> _logger;

    public GymService(
        IGymRepository repo,
        IUnitOfWork unitOfWork,
        ILogger<GymService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateGymDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var gymResult = Gym.Create(dto.Name);
            if (gymResult.IsFailure)
            {
                if (gymResult.Error is ValidationErrors ve)
                {
                    errors.AddRange(ve.Errors);
                }
                else
                {
                    errors.Add(gymResult.Error);
                }
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(gymResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(gymResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a gym.");
            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int gymId,
        UpdateGymDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(gymId);
            if (idResult.IsFailure)
            {
                if (idResult.Error is ValidationErrors ve)
                {
                    errors.AddRange(ve.Errors);
                }
                else
                {
                    errors.Add(idResult.Error);
                }
            }

            var gym = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (gym is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var gymResult = gym.Update(dto.Name);
            if (gymResult.IsFailure)
            {
                if (gymResult.Error is ValidationErrors ve)
                {
                    errors.AddRange(ve.Errors);
                }
                else
                {
                    errors.Add(gymResult.Error);
                }
            }

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating gym {GymId}.", gymId);
            return Result.Failure(ServiceErrors.Common.UnExcepected);
        }
    }
}

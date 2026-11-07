using GYM.Application.Abstractions;
using GYM.Application.DTOs.Exercise;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class ExerciseService :
    ServiceBase<Exercise, ExerciseResponseDto, CreateExerciseDto, ExerciseUpdateDto>,
    IExerciseService
{
    private readonly IExerciseRepository _repo;
    private readonly IGymRepository _gymRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(
        IExerciseRepository repo,
        IGymRepository gymRepo,
        IUnitOfWork unitOfWork,
        ILogger<ExerciseService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _gymRepo = gymRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateExerciseDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var gymIdResult = Id.Create(dto.GymId);
            if (gymIdResult.IsFailure)
                errors.Add(gymIdResult.Error);

            if (gymIdResult.IsSuccess)
            {
                var gymExists = await _gymRepo.ExistsAsync(gymIdResult.Value, cancellationToken);
                if (!gymExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            var name = dto.Name.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var existsByName = await _repo.ExistsByNameAsync(name, cancellationToken);
                if (existsByName)
                    errors.Add(ServiceErrors.Exercise.Name.AlreadyExists);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var exerciseResult = Exercise.Create(
                gymIdResult.Value,
                name,
                dto.MuscleGroup,
                dto.Description,
                dto.Instructions,
                dto.VideoUrl);

            if (exerciseResult.IsFailure)
            {
                if (exerciseResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(exerciseResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(exerciseResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(exerciseResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating an exercise.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int exerciseId,
        ExerciseUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(exerciseId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var exercise = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (exercise is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var name = dto.Name.Trim();
            var existingByName = await _repo.GetByNameAsync(name, cancellationToken);
            if (existingByName is not null && existingByName.Id != exercise.Id)
                errors.Add(ServiceErrors.Exercise.Name.AlreadyExists);

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            var updateResult = exercise.Update(
                name,
                dto.MuscleGroup,
                dto.Description,
                dto.Instructions,
                dto.VideoUrl,
                dto.IsActive);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating exercise {ExerciseId}.", exerciseId);
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}

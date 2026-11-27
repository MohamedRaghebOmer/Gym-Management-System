using GYM.Application.Abstractions;
using GYM.Application.DTOs.WorkoutPlanExercise;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class WorkoutPlanExerciseService
    : ServiceBase<WorkoutPlanExercise, WorkoutPlanExerciseResponseDto, CreateWorkoutPlanExerciseDto, WorkoutPlanExerciseUpdateDto>,
      IWorkoutPlanExerciseService
{
    private readonly IWorkoutPlanExerciseRepository _repo;
    private readonly IWorkoutPlanRepository _workoutPlanRepo;
    private readonly IExerciseRepository _exerciseRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkoutPlanExerciseService> _logger;

    public WorkoutPlanExerciseService(
        IWorkoutPlanExerciseRepository repo,
        IWorkoutPlanRepository workoutPlanRepo,
        IExerciseRepository exerciseRepo,
        IUnitOfWork unitOfWork,
        ILogger<WorkoutPlanExerciseService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _workoutPlanRepo = workoutPlanRepo;
        _exerciseRepo = exerciseRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateWorkoutPlanExerciseDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var workoutPlanIdResult = Id.Create(dto.WorkoutPlanId);
            if (workoutPlanIdResult.IsFailure)
                errors.Add(workoutPlanIdResult.Error);

            var exerciseIdResult = Id.Create(dto.ExerciseId);
            if (exerciseIdResult.IsFailure)
                errors.Add(exerciseIdResult.Error);

            if (workoutPlanIdResult.IsSuccess)
            {
                var workoutPlanExists = await _workoutPlanRepo.ExistsAsync(workoutPlanIdResult.Value, cancellationToken);
                if (!workoutPlanExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            if (exerciseIdResult.IsSuccess)
            {
                var exerciseExists = await _exerciseRepo.ExistsAsync(exerciseIdResult.Value, cancellationToken);
                if (!exerciseExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            if (workoutPlanIdResult.IsSuccess && exerciseIdResult.IsSuccess)
            {
                var alreadyExists = await _repo.ExistsAsync(
                    workoutPlanIdResult.Value,
                    exerciseIdResult.Value,
                    cancellationToken);

                if (alreadyExists)
                    errors.Add(ServiceErrors.WorkoutPlanExercise.AlreadyExists);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var workoutPlanExerciseResult = WorkoutPlanExercise.Create(
                workoutPlanIdResult.Value,
                exerciseIdResult.Value,
                dto.Sets,
                dto.Reps,
                dto.Weight,
                dto.RestSeconds,
                dto.DisplayOrder,
                dto.Notes);

            if (workoutPlanExerciseResult.IsFailure)
            {
                if (workoutPlanExerciseResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(workoutPlanExerciseResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(workoutPlanExerciseResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(workoutPlanExerciseResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a workout plan exercise.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int id,
        WorkoutPlanExerciseUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var idResult = Id.Create(id);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var workoutPlanExercise = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (workoutPlanExercise is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var updateResult = workoutPlanExercise.Update(
                dto.Sets,
                dto.Reps,
                dto.Weight,
                dto.RestSeconds,
                dto.DisplayOrder,
                dto.Notes);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating workout plan exercise {WorkoutPlanExerciseId}.", id);
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}

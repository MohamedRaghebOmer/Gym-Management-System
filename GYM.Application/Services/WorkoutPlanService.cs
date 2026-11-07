using GYM.Application.Abstractions;
using GYM.Application.DTOs.WorkoutPlan;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class WorkoutPlanService : ServiceBase<WorkoutPlan, WorkoutResponseDto, CreateWorkoutPlanDto, UpdateWorkoutPlanDto>, IWorkoutPlanService
{
    private readonly IWorkoutPlanRepository _repo;
    private readonly IUnitOfWork _unitOfwork;
    private readonly ILogger<WorkoutPlanService> _logger;

    public WorkoutPlanService(
        IWorkoutPlanRepository repo,
        IUnitOfWork unitOfwork,
        ILogger<WorkoutPlanService> logger)
        : base(repo, unitOfwork, logger)
    {
        _repo = repo;
        _unitOfwork = unitOfwork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateWorkoutPlanDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var memberIdResult = Id.Create(dto.MemberId);
            if (memberIdResult.IsFailure)
                errors.Add(memberIdResult.Error);

            var trainterIdResult = Id.Create(dto.TrainerId);
            if (trainterIdResult.IsFailure)
                errors.Add(trainterIdResult.Error);

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var workoutPlanResult = WorkoutPlan.Create(
                memberIdResult.Value,
                trainterIdResult.Value,
                dto.Name,
                dto.Description,
                dto.StartDate,
                dto.EndDate);

            if (workoutPlanResult.IsFailure)
            {
                if (workoutPlanResult.Error is ValidationErrors ve)
                {
                    errors.AddRange(ve.Errors);
                }
                else
                {
                    errors.Add(workoutPlanResult.Error);
                }
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(workoutPlanResult.Value);
            await _unitOfwork.SaveChangesAsync(cancellationToken);

            return Result.Success(workoutPlanResult.Value.Id.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while creating a workout plan.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int id,
        UpdateWorkoutPlanDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var idResult = Id.Create(id);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var workoutPlan = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (workoutPlan is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var updateResult = workoutPlan.Update(
                dto.Name,
                dto.Description,
                dto.StartDate,
                dto.EndDate,
                dto.IsActive);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfwork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while updating a workout plan.");
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}
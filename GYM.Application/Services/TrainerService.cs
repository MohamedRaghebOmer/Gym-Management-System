using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Trainer;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class TrainerService :
    ServiceBase<Trainer, TrainerResponseDto, CreateTrainerDto, TrainerUpdateDto>,
    ITrainerService
{
    private readonly ITrainerRepository _repo;
    private readonly IPersonRepository _personRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TrainerService> _logger;

    public TrainerService(
        ITrainerRepository repo,
        IPersonRepository personRepo,
        IUnitOfWork unitOfWork,
        ILogger<TrainerService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _personRepo = personRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateTrainerDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var personIdResult = Id.Create(dto.PersonId);
            if (personIdResult.IsFailure)
                errors.Add(personIdResult.Error);

            if (personIdResult.IsSuccess)
            {
                var personExists = await _personRepo.ExistsAsync(
                    personIdResult.Value,
                    cancellationToken);

                if (!personExists)
                    errors.Add(ServiceErrors.Common.NotFound);

                var trainerExists = await _repo.ExistsByPersonIdAsync(
                    personIdResult.Value,
                    cancellationToken);

                if (trainerExists)
                    errors.Add(ServiceErrors.Trainer.Person.AlreadyAssociatedWithTrainer);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var trainerResult = Trainer.Create(
                personIdResult.Value,
                dto.Specialization,
                dto.HireDate,
                dto.Notes);

            if (trainerResult.IsFailure)
            {
                if (trainerResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(trainerResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(trainerResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(trainerResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a trainer.");
            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int trainerId,
        TrainerUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var idResult = Id.Create(trainerId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var trainer = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (trainer is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var updateResult = trainer.Update(
                dto.Specialization,
                dto.IsActive,
                dto.Notes);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating trainer {TrainerId}.", trainerId);
            return Result.Failure(ServiceErrors.Common.UnExcepected);
        }
    }
}

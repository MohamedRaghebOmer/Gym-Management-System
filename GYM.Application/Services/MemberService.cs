using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Member;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class MemberService :
    ServiceBase<Member, MemberResponseDto, CreateMemberDto, MemberUpdateDto>,
    IMemberService
{
    private readonly IMemberRepository _repo;
    private readonly IPersonRepository _personRepo;
    private readonly ITrainerRepository _trainerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MemberService> _logger;

    public MemberService(
        IMemberRepository repo,
        IPersonRepository personRepo,
        ITrainerRepository trainerRepo,
        IUnitOfWork unitOfWork,
        ILogger<MemberService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _personRepo = personRepo;
        _trainerRepo = trainerRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateMemberDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var personIdResult = Id.Create(dto.PersonId);
            if (personIdResult.IsFailure)
                errors.Add(personIdResult.Error);

            Result<Id>? trainerIdResult = null;
            if (dto.TrainerId.HasValue)
            {
                trainerIdResult = Id.Create(dto.TrainerId.Value);
                if (trainerIdResult.IsFailure)
                    errors.Add(trainerIdResult.Error);
            }

            if (personIdResult.IsSuccess)
            {
                var personExists = await _personRepo.ExistsAsync(personIdResult.Value, cancellationToken);
                if (!personExists)
                    errors.Add(ServiceErrors.Common.NotFound);

                var memberExists = await _repo.ExistsByPersonIdAsync(personIdResult.Value, cancellationToken);
                if (memberExists)
                    errors.Add(ServiceErrors.Member.Person.AlreadyAssociatedWithMember);
            }

            if (trainerIdResult?.IsSuccess == true)
            {
                var trainerExists = await _trainerRepo.ExistsAsync(trainerIdResult.Value, cancellationToken);
                if (!trainerExists)
                    errors.Add(ServiceErrors.Common.NotFound);

                var memberExistsByTrainer = await _repo.ExistsByTrainerIdAsync(trainerIdResult.Value, cancellationToken);
                if (memberExistsByTrainer)
                    errors.Add(ServiceErrors.Member.Trainer.AlreadyAssociatedWithMember);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var memberResult = Member.Create(
                personIdResult.Value,
                trainerIdResult?.Value,
                dto.EmergencyContactName,
                dto.EmergencyContactPhone,
                dto.JoinDate,
                dto.Notes);

            if (memberResult.IsFailure)
            {
                if (memberResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(memberResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(memberResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(memberResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a member.");
            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int memberId,
        MemberUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(memberId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var member = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (member is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            Result<Id>? trainerIdResult = null;
            if (dto.TrainerId.HasValue)
            {
                trainerIdResult = Id.Create(dto.TrainerId.Value);
                if (trainerIdResult.IsFailure)
                    errors.Add(trainerIdResult.Error);

                if (trainerIdResult?.IsSuccess == true)
                {
                    var trainerExists = await _trainerRepo.ExistsAsync(trainerIdResult.Value, cancellationToken);
                    if (!trainerExists)
                        errors.Add(ServiceErrors.Common.NotFound);

                    var existingMemberByTrainer = await _repo.GetByTrainerIdAsync(trainerIdResult.Value, cancellationToken);
                    if (existingMemberByTrainer is not null && existingMemberByTrainer.Id != member.Id)
                        errors.Add(ServiceErrors.Member.Trainer.AlreadyAssociatedWithMember);
                }
            }

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            var updateResult = member.Update(
                trainerIdResult?.Value,
                dto.EmergencyContactName,
                dto.EmergencyContactPhone,
                dto.IsActive,
                dto.Notes);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating member {MemberId}.", memberId);
            return Result.Failure(ServiceErrors.Common.UnExcepected);
        }
    }
}

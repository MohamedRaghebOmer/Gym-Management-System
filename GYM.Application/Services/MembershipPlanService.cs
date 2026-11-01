using GYM.Application.Abstractions;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.MembershipPlan;
using GYM.Application.Errors;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class MembershipPlanService :
    ServiceBase<MembershipPlan, MembershipPlanResponseDto, CreateMembershipPlanDto, MembershipPlanUpdateDto>,
    IMembershipPlanService
{
    private readonly IMembershipPlanRepository _repo;
    private readonly IGymRepository _gymRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MembershipPlanService> _logger;

    public MembershipPlanService(
        IMembershipPlanRepository repo,
        IGymRepository gymRepo,
        IUnitOfWork unitOfWork,
        ILogger<MembershipPlanService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _gymRepo = gymRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateMembershipPlanDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var gymIdResult = Id.Create(dto.GymId);
            if (gymIdResult.IsFailure)
                errors.Add(gymIdResult.Error);

            var priceResult = Money.Create(dto.Price);
            if (priceResult.IsFailure)
                errors.Add(priceResult.Error);

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
                    errors.Add(ServiceErrors.MembershipPlan.Name.AlreadyExists);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var membershipPlanResult = MembershipPlan.Create(
                gymIdResult.Value,
                name,
                dto.DurationInDays,
                priceResult.Value,
                dto.Description);

            if (membershipPlanResult.IsFailure)
            {
                if (membershipPlanResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(membershipPlanResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(membershipPlanResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(membershipPlanResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a membership plan.");
            return Result.Failure<int>(ServiceErrors.Common.UnExcepected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int membershipPlanId,
        MembershipPlanUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(membershipPlanId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var membershipPlan = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (membershipPlan is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var priceResult = Money.Create(dto.Price);
            if (priceResult.IsFailure)
                errors.Add(priceResult.Error);

            var name = dto.Name.Trim();
            var existingByName = await _repo.GetByNameAsync(name, cancellationToken);
            if (existingByName is not null && existingByName.Id != membershipPlan.Id)
                errors.Add(ServiceErrors.MembershipPlan.Name.AlreadyExists);

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            var updateResult = membershipPlan.Update(
                name,
                dto.DurationInDays,
                priceResult.Value,
                dto.Description,
                dto.IsActive);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating membership plan {MembershipPlanId}.", membershipPlanId);
            return Result.Failure(ServiceErrors.Common.UnExcepected);
        }
    }
}

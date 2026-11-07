using GYM.Application.Abstractions;
using GYM.Application.DTOs.Subscription;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class SubscriptionService :
    ServiceBase<Subscription, SubscriptionResponseDto, CreateSubscriptionDto, SubscriptionUpdateDto>,
    ISubscriptionService
{
    private readonly ISubscriptionRepository _repo;
    private readonly IMemberRepository _memberRepo;
    private readonly IMembershipPlanRepository _membershipPlanRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ISubscriptionRepository repo,
        IMemberRepository memberRepo,
        IMembershipPlanRepository membershipPlanRepo,
        IUnitOfWork unitOfWork,
        ILogger<SubscriptionService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _memberRepo = memberRepo;
        _membershipPlanRepo = membershipPlanRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreateSubscriptionDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var memberIdResult = Id.Create(dto.MemberId);
            if (memberIdResult.IsFailure)
                errors.Add(memberIdResult.Error);

            var membershipPlanIdResult = Id.Create(dto.MembershipPlanId);
            if (membershipPlanIdResult.IsFailure)
                errors.Add(membershipPlanIdResult.Error);

            var priceResult = Money.Create(dto.Price);
            if (priceResult.IsFailure)
                errors.Add(priceResult.Error);

            if (memberIdResult.IsSuccess)
            {
                var memberExists =
                    await _memberRepo.ExistsAsync(
                        memberIdResult.Value, cancellationToken);

                if (!memberExists)
                    errors.Add(ServiceErrors.Common.NotFound);

                var hasOverlappingActiveSubscription =
                    await _repo.HasOverlappingActiveSubscriptionAsync(
                        memberIdResult.Value,
                        dto.StartDate,
                        dto.EndDate,
                        null,
                        cancellationToken);

                if (hasOverlappingActiveSubscription)
                    errors.Add(ServiceErrors.Subscription.Member.OverlappingActiveSubscription);
            }

            if (membershipPlanIdResult.IsSuccess)
            {
                var membershipPlanExists =
                    await _membershipPlanRepo.ExistsAsync(
                        membershipPlanIdResult.Value, cancellationToken);

                if (!membershipPlanExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var subscriptionResult = Subscription.Create(
                memberIdResult.Value,
                membershipPlanIdResult.Value,
                dto.StartDate,
                dto.EndDate,
                priceResult.Value);

            if (subscriptionResult.IsFailure)
            {
                if (subscriptionResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(subscriptionResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(subscriptionResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(subscriptionResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a subscription.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int subscriptionId,
        SubscriptionUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var idResult = Id.Create(subscriptionId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var subscription = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (subscription is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var priceResult = Money.Create(dto.Price);
            if (priceResult.IsFailure)
                errors.Add(priceResult.Error);

            if (dto.Status == SubscriptionStatus.Active)
            {
                var hasOverlappingActiveSubscription =
                    await _repo.HasOverlappingActiveSubscriptionAsync(
                        subscription.MemberId,
                        dto.StartDate,
                        dto.EndDate,
                        subscription.Id,
                        cancellationToken);

                if (hasOverlappingActiveSubscription)
                    errors.Add(ServiceErrors.Subscription.Member.OverlappingActiveSubscription);
            }

            if (errors.Count > 0)
                return Result.Failure(new ValidationErrors(errors.ToArray()));

            var updateResult = subscription.Update(
                dto.StartDate,
                dto.EndDate,
                priceResult.Value,
                dto.Status);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating subscription {SubscriptionId}.", subscriptionId);
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}

using GYM.Application.Abstractions;
using GYM.Application.DTOs.Payment;
using GYM.Application.Errors;
using GYM.Application.ServicesInterfaces;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GYM.Application.Services;

public sealed class PaymentService :
    ServiceBase<Payment, PaymentResponseDto, CreatePaymentDto, PaymentUpdateDto>,
    IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository repo,
        ISubscriptionRepository subscriptionRepo,
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger)
        : base(repo, unitOfWork, logger)
    {
        _repo = repo;
        _subscriptionRepo = subscriptionRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<int>> CreateAsync(
        CreatePaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Error> errors = [];

            var subscriptionIdResult = Id.Create(dto.SubscriptionId);
            if (subscriptionIdResult.IsFailure)
                errors.Add(subscriptionIdResult.Error);

            var amountResult = Money.Create(dto.Amount);
            if (amountResult.IsFailure)
                errors.Add(amountResult.Error);

            if (subscriptionIdResult.IsSuccess)
            {
                var subscriptionExists = await _subscriptionRepo.ExistsAsync(
                    subscriptionIdResult.Value,
                    cancellationToken);

                if (!subscriptionExists)
                    errors.Add(ServiceErrors.Common.NotFound);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            var paymentResult = Payment.Create(
                subscriptionIdResult.Value,
                amountResult.Value,
                dto.PaymentMethod,
                dto.PaymentDate,
                dto.Notes);

            if (paymentResult.IsFailure)
            {
                if (paymentResult.Error is ValidationErrors validationErrors)
                    errors.AddRange(validationErrors.Errors);
                else
                    errors.Add(paymentResult.Error);
            }

            if (errors.Count > 0)
                return Result.Failure<int>(new ValidationErrors(errors.ToArray()));

            _repo.Add(paymentResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(paymentResult.Value.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating a payment.");
            return Result.Failure<int>(ServiceErrors.Common.Unexpected);
        }
    }

    public override async Task<Result> UpdateAsync(
        int paymentId,
        PaymentUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var idResult = Id.Create(paymentId);
            if (idResult.IsFailure)
                return Result.Failure(idResult.Error);

            var payment = await _repo.GetByIdAsync(idResult.Value, cancellationToken);
            if (payment is null)
                return Result.Failure(ServiceErrors.Common.NotFound);

            var updateResult = payment.Update(
                dto.Notes);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating payment {PaymentId}.", paymentId);
            return Result.Failure(ServiceErrors.Common.Unexpected);
        }
    }
}

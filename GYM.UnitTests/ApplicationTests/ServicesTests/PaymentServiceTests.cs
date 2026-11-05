using FluentAssertions;
using GYM.Application.DTOs.Payment;
using GYM.Application.Errors;
using GYM.Application.Services;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidSubscriptionIdAndAmount_ReturnsValidationFailureWithoutPersisting()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var dto = new CreatePaymentDto
        {
            SubscriptionId = 0,
            Amount = 0,
            PaymentMethod = PaymentMethod.Cash,
            PaymentDate = DateTime.UtcNow,
            Notes = "note"
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");
        validationErrors.Errors.Should().Contain(error => error.Code == "Money.LessThanOrEqualToZero");

        paymentRepository.Verify(repository => repository.Add(It.IsAny<Payment>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SubscriptionNotFound_ReturnsValidationFailureWithoutPersisting()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var dto = new CreatePaymentDto
        {
            SubscriptionId = 5,
            Amount = 100m,
            PaymentMethod = PaymentMethod.Card,
            PaymentDate = DateTime.UtcNow,
            Notes = "note"
        };

        subscriptionRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.SubscriptionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == ServiceErrors.Common.NotFound.Code);

        paymentRepository.Verify(repository => repository.Add(It.IsAny<Payment>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidPaymentPayload_ReturnsValidationFailureWithoutPersisting()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var dto = new CreatePaymentDto
        {
            SubscriptionId = 7,
            Amount = 100m,
            PaymentMethod = (PaymentMethod)255,
            PaymentDate = default,
            Notes = new string('x', Payment.Constants.NotesMaxLength + 1)
        };

        subscriptionRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.SubscriptionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Payment.PaymentMethod.Invalid");
        validationErrors.Errors.Should().Contain(error => error.Code == "Payment.PaymentDate.Required");
        validationErrors.Errors.Should().Contain(error => error.Code == "Payment.Notes.TooLong");

        paymentRepository.Verify(repository => repository.Add(It.IsAny<Payment>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsPaymentAndReturnsId()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var dto = new CreatePaymentDto
        {
            SubscriptionId = 3,
            Amount = 99.99m,
            PaymentMethod = PaymentMethod.BankTransfer,
            PaymentDate = new DateTime(2026, 2, 2, 10, 0, 0, DateTimeKind.Utc),
            Notes = "monthly payment"
        };

        subscriptionRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.SubscriptionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Payment? addedEntity = null;
        paymentRepository
            .Setup(repository => repository.Add(It.IsAny<Payment>()))
            .Callback<Payment>(entity =>
            {
                addedEntity = entity;
                typeof(Payment)
                    .BaseType!
                    .GetProperty(nameof(Payment.Id))!
                    .SetValue(entity, Id.FromDatabase(44));
            });

        var result = await service.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(44);

        addedEntity.Should().NotBeNull();
        addedEntity.SubscriptionId.Value.Should().Be(dto.SubscriptionId);
        addedEntity.Amount.Value.Should().Be(dto.Amount);
        addedEntity.PaymentMethod.Should().Be(dto.PaymentMethod);
        addedEntity.PaymentDate.Should().Be(dto.PaymentDate);
        addedEntity.Notes.Should().Be(dto.Notes);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutSaving()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var result = await service.UpdateAsync(0, new PaymentUpdateDto { Notes = "note" });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");

        paymentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PaymentNotFound_ReturnsNotFound()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        paymentRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 8),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await service.UpdateAsync(8, new PaymentUpdateDto { Notes = "note" });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ServiceErrors.Common.NotFound.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NotesTooLong_ReturnsFailureWithoutSaving()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var payment = Payment.Create(
            Id.Create(3).Value,
            Money.Create(50m).Value,
            PaymentMethod.Cash,
            DateTime.UtcNow,
            "initial").Value;

        paymentRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await service.UpdateAsync(
            9,
            new PaymentUpdateDto
            {
                Notes = new string('x', Payment.Constants.NotesMaxLength + 1)
            });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.Notes.TooLong");
        payment.Notes.Should().Be("initial");

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesNotesAndSavesChanges()
    {
        var paymentRepository = new Mock<IPaymentRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(paymentRepository, subscriptionRepository, unitOfWork);

        var payment = Payment.Create(
            Id.Create(3).Value,
            Money.Create(50m).Value,
            PaymentMethod.Cash,
            DateTime.UtcNow,
            "initial").Value;

        paymentRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 11),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await service.UpdateAsync(
            11,
            new PaymentUpdateDto
            {
                Notes = "  updated note  "
            });

        result.IsSuccess.Should().BeTrue();
        payment.Notes.Should().Be("updated note");

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static PaymentService CreateService(
        Mock<IPaymentRepository> paymentRepository,
        Mock<ISubscriptionRepository> subscriptionRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new PaymentService(
            paymentRepository.Object,
            subscriptionRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<PaymentService>>());
    }
}

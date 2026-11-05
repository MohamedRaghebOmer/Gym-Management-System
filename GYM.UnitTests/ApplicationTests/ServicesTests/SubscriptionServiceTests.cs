using FluentAssertions;
using GYM.Application.DTOs.Subscription;
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

public sealed class SubscriptionServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidMemberIdMembershipPlanIdAndPrice_ReturnsValidationFailureWithoutPersisting()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(subscriptionRepository, memberRepository, membershipPlanRepository, unitOfWork);

        var dto = new CreateSubscriptionDto
        {
            MemberId = 0,
            MembershipPlanId = 0,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 2, 1),
            Price = 0
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");
        validationErrors.Errors.Should().Contain(error => error.Code == "Money.LessThanOrEqualToZero");

        subscriptionRepository.Verify(repository => repository.Add(It.IsAny<Subscription>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_OverlappingActiveSubscription_ReturnsValidationFailureWithoutPersisting()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(subscriptionRepository, memberRepository, membershipPlanRepository, unitOfWork);

        var dto = new CreateSubscriptionDto
        {
            MemberId = 3,
            MembershipPlanId = 8,
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2026, 5, 1),
            Price = 100
        };

        memberRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        subscriptionRepository
            .Setup(repository => repository.HasOverlappingActiveSubscriptionAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                dto.StartDate,
                dto.EndDate,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        membershipPlanRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MembershipPlanId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Subscription.Member.OverlappingActiveSubscription.Code);

        subscriptionRepository.Verify(repository => repository.Add(It.IsAny<Subscription>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsSubscriptionAndReturnsId()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(subscriptionRepository, memberRepository, membershipPlanRepository, unitOfWork);

        var dto = new CreateSubscriptionDto
        {
            MemberId = 3,
            MembershipPlanId = 8,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 7, 1),
            Price = 250m
        };

        memberRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        subscriptionRepository
            .Setup(repository => repository.HasOverlappingActiveSubscriptionAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                dto.StartDate,
                dto.EndDate,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        membershipPlanRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MembershipPlanId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Subscription? addedEntity = null;
        subscriptionRepository
            .Setup(repository => repository.Add(It.IsAny<Subscription>()))
            .Callback<Subscription>(entity =>
            {
                addedEntity = entity;
                typeof(Subscription)
                    .BaseType!
                    .GetProperty(nameof(Subscription.Id))!
                    .SetValue(entity, Id.FromDatabase(55));
            });

        var result = await service.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(55);

        addedEntity.Should().NotBeNull();
        addedEntity.MemberId.Value.Should().Be(dto.MemberId);
        addedEntity.MembershipPlanId.Value.Should().Be(dto.MembershipPlanId);
        addedEntity.StartDate.Should().Be(dto.StartDate);
        addedEntity.EndDate.Should().Be(dto.EndDate);
        addedEntity.Price.Value.Should().Be(dto.Price);
        addedEntity.Status.Should().Be(SubscriptionStatus.Active);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutSaving()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(subscriptionRepository, memberRepository, membershipPlanRepository, unitOfWork);

        var dto = new SubscriptionUpdateDto
        {
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 2, 1),
            Price = 120,
            Status = SubscriptionStatus.Active
        };

        var result = await service.UpdateAsync(0, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");

        subscriptionRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ActiveStatusWithOverlappingSubscription_ReturnsValidationFailureWithoutSaving()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(subscriptionRepository, memberRepository, membershipPlanRepository, unitOfWork);

        var subscription = Subscription.Create(
            Id.Create(5).Value,
            Id.Create(12).Value,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 1),
            Money.Create(100).Value).Value;

        typeof(Subscription)
            .BaseType!
            .GetProperty(nameof(Subscription.Id))!
            .SetValue(subscription, Id.FromDatabase(77));

        subscriptionRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 77),
                cancellationToken))
            .ReturnsAsync(subscription);

        subscriptionRepository
            .Setup(repository => repository.HasOverlappingActiveSubscriptionAsync(
                subscription.MemberId,
                new DateOnly(2026, 5, 1),
                new DateOnly(2026, 6, 1),
                subscription.Id,
                cancellationToken))
            .ReturnsAsync(true);

        var dto = new SubscriptionUpdateDto
        {
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 6, 1),
            Price = 130,
            Status = SubscriptionStatus.Active
        };

        var result = await service.UpdateAsync(77, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Subscription.Member.OverlappingActiveSubscription.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEntityAndSavesChanges()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(subscriptionRepository, memberRepository, membershipPlanRepository, unitOfWork);

        var subscription = Subscription.Create(
            Id.Create(5).Value,
            Id.Create(12).Value,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 1),
            Money.Create(100).Value).Value;

        subscriptionRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 88),
                cancellationToken))
            .ReturnsAsync(subscription);

        var dto = new SubscriptionUpdateDto
        {
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 8, 1),
            Price = 180,
            Status = SubscriptionStatus.Cancelled
        };

        var result = await service.UpdateAsync(88, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        subscription.StartDate.Should().Be(dto.StartDate);
        subscription.EndDate.Should().Be(dto.EndDate);
        subscription.Price.Value.Should().Be(dto.Price);
        subscription.Status.Should().Be(dto.Status);
        subscription.UpdatedAt.Should().NotBeNull();

        subscriptionRepository.Verify(repository => repository.HasOverlappingActiveSubscriptionAsync(
            It.IsAny<Id>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>(),
            It.IsAny<Id?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static SubscriptionService CreateService(
        Mock<ISubscriptionRepository> subscriptionRepository,
        Mock<IMemberRepository> memberRepository,
        Mock<IMembershipPlanRepository> membershipPlanRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new SubscriptionService(
            subscriptionRepository.Object,
            memberRepository.Object,
            membershipPlanRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<SubscriptionService>>());
    }
}

using FluentAssertions;
using GYM.Application.DTOs.MembershipPlan;
using GYM.Application.Errors;
using GYM.Application.Services;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class MembershipPlanServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidGymIdAndPrice_ReturnsValidationFailureWithoutPersisting()
    {
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(membershipPlanRepository, gymRepository, unitOfWork);

        var dto = new CreateMembershipPlanDto
        {
            GymId = 0,
            Name = "Basic",
            DurationInDays = 30,
            Price = 0,
            Description = "desc"
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");
        validationErrors.Errors.Should().Contain(error => error.Code == "Money.LessThanOrEqualToZero");

        membershipPlanRepository.Verify(repository => repository.Add(It.IsAny<MembershipPlan>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NameAlreadyExists_ReturnsValidationFailureWithoutPersisting()
    {
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(membershipPlanRepository, gymRepository, unitOfWork);

        var dto = new CreateMembershipPlanDto
        {
            GymId = 2,
            Name = "Gold",
            DurationInDays = 30,
            Price = 100,
            Description = "desc"
        };

        gymRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.GymId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        membershipPlanRepository
            .Setup(repository => repository.ExistsByNameAsync("Gold", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.MembershipPlan.Name.AlreadyExists.Code);

        membershipPlanRepository.Verify(repository => repository.Add(It.IsAny<MembershipPlan>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsMembershipPlanAndReturnsId()
    {
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(membershipPlanRepository, gymRepository, unitOfWork);

        var dto = new CreateMembershipPlanDto
        {
            GymId = 4,
            Name = "  Premium  ",
            DurationInDays = 60,
            Price = 249.99m,
            Description = "  Access all classes  "
        };

        gymRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.GymId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        membershipPlanRepository
            .Setup(repository => repository.ExistsByNameAsync("Premium", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MembershipPlan? addedEntity = null;
        membershipPlanRepository
            .Setup(repository => repository.Add(It.IsAny<MembershipPlan>()))
            .Callback<MembershipPlan>(entity =>
            {
                addedEntity = entity;
                typeof(MembershipPlan)
                    .BaseType!
                    .GetProperty(nameof(MembershipPlan.Id))!
                    .SetValue(entity, Id.FromDatabase(77));
            });

        var result = await service.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(77);
        addedEntity.Should().NotBeNull();
        addedEntity.Name.Should().Be("Premium");
        addedEntity.Description.Should().Be("Access all classes");
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutSaving()
    {
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(membershipPlanRepository, gymRepository, unitOfWork);

        var dto = new MembershipPlanUpdateDto
        {
            Name = "Basic",
            DurationInDays = 30,
            Price = 99,
            Description = "desc",
            IsActive = true
        };

        var result = await service.UpdateAsync(0, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");
        membershipPlanRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PlanNotFound_ReturnsNotFoundWithoutSaving()
    {
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(membershipPlanRepository, gymRepository, unitOfWork);

        membershipPlanRepository
            .Setup(repository => repository.GetByIdAsync(It.Is<Id>(id => id.Value == 11), cancellationToken))
            .ReturnsAsync((MembershipPlan?)null);

        var dto = new MembershipPlanUpdateDto
        {
            Name = "Basic",
            DurationInDays = 30,
            Price = 99,
            Description = "desc",
            IsActive = true
        };

        var result = await service.UpdateAsync(11, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ServiceErrors.Common.NotFound.Code);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEntityAndSavesChanges()
    {
        var membershipPlanRepository = new Mock<IMembershipPlanRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(membershipPlanRepository, gymRepository, unitOfWork);

        var plan = MembershipPlan.Create(
            Id.Create(3).Value,
            "Basic",
            30,
            Money.Create(99).Value,
            "desc").Value;

        membershipPlanRepository
            .Setup(repository => repository.GetByIdAsync(It.Is<Id>(id => id.Value == 11), cancellationToken))
            .ReturnsAsync(plan);

        membershipPlanRepository
            .Setup(repository => repository.GetByNameAsync("Premium", cancellationToken))
            .ReturnsAsync((MembershipPlan?)null);

        var dto = new MembershipPlanUpdateDto
        {
            Name = "  Premium  ",
            DurationInDays = 90,
            Price = 199.99m,
            Description = "  Updated desc  ",
            IsActive = false
        };

        var result = await service.UpdateAsync(11, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        plan.Name.Should().Be("Premium");
        plan.DurationInDays.Should().Be(90);
        plan.Price.Value.Should().Be(199.99m);
        plan.Description.Should().Be("Updated desc");
        plan.IsActive.Should().BeFalse();
        plan.UpdatedAt.Should().NotBeNull();

        unitOfWork.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static MembershipPlanService CreateService(
        Mock<IMembershipPlanRepository> membershipPlanRepository,
        Mock<IGymRepository> gymRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new MembershipPlanService(
            membershipPlanRepository.Object,
            gymRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<MembershipPlanService>>());
    }
}

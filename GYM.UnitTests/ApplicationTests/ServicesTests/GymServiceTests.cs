using FluentAssertions;
using GYM.Application.DTOs.Gym;
using GYM.Application;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class GymServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidName_ReturnsValidationFailureWithoutPersisting()
    {
        var repository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var dto = new CreateGymDto { Name = "A" };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validation = result.Error.Should().BeOfType<GYM.Domain.Shared.ValidationErrors>().Subject;
        validation.Errors.Should().ContainSingle(e => e.Code == "Gym.Name.TooShort");
        repository.Verify(r => r.Add(It.IsAny<Gym>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingGym_UpdatesEntityAndSavesChanges()
    {
        var repository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var gym = Gym.Create("Original").Value;
        var cancellationToken = new CancellationTokenSource().Token;

        repository
            .Setup(r => r.GetByIdAsync(It.Is<GYM.Domain.ValueObjects.Id>(id => id.Value == 12), cancellationToken))
            .ReturnsAsync(gym);

        var dto = new UpdateGymDto { Name = "Updated Name" };

        var result = await service.UpdateAsync(12, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        gym.Name.Should().Be("Updated Name");
        repository.Verify(r => r.GetByIdAsync(It.Is<GYM.Domain.ValueObjects.Id>(id => id.Value == 12), cancellationToken), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_GymDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var repository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = new UpdateGymDto { Name = "Name" };

        repository
            .Setup(r => r.GetByIdAsync(It.Is<GYM.Domain.ValueObjects.Id>(id => id.Value == 12), cancellationToken))
            .ReturnsAsync((Gym?)null);

        var result = await service.UpdateAsync(12, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Common.NotFound");
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static IGymService CreateService(
        Mock<IGymRepository> repository,
        Mock<IUnitOfWork> unitOfWork)
    {
        var provider = new ServiceCollection()
            .AddSingleton(repository.Object)
            .AddSingleton(unitOfWork.Object)
            .AddLogging()
            .AddApplication()
            .BuildServiceProvider();

        return provider.GetRequiredService<IGymService>();
    }
}

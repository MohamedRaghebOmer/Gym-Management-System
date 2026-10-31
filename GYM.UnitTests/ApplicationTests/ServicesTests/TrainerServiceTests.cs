using FluentAssertions;
using GYM.Application.DTOs.Trainer;
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

public sealed class TrainerServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidPersonId_ReturnsValidationFailureWithoutPersisting()
    {
        var trainerRepository = new Mock<ITrainerRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(trainerRepository, personRepository, unitOfWork);

        var dto = new CreateTrainerDto
        {
            PersonId = 0,
            Specialization = "Strength",
            HireDate = DateTime.UtcNow.AddDays(-5),
            Notes = "Notes"
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");

        trainerRepository.Verify(repository => repository.Add(It.IsAny<Trainer>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PersonAlreadyAssociatedWithTrainer_ReturnsValidationFailureWithoutPersisting()
    {
        var trainerRepository = new Mock<ITrainerRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(trainerRepository, personRepository, unitOfWork);

        var dto = new CreateTrainerDto
        {
            PersonId = 5,
            Specialization = "Strength",
            HireDate = DateTime.UtcNow.AddDays(-2),
            Notes = "Notes"
        };

        personRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        trainerRepository
            .Setup(repository => repository.ExistsByPersonIdAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Trainer.Person.AlreadyAssociatedWithTrainer.Code);

        trainerRepository.Verify(repository => repository.Add(It.IsAny<Trainer>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutUpdating()
    {
        var trainerRepository = new Mock<ITrainerRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(trainerRepository, personRepository, unitOfWork);

        var dto = new TrainerUpdateDto
        {
            Specialization = "Updated",
            IsActive = true,
            Notes = "Updated notes"
        };

        var result = await service.UpdateAsync(0, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");

        trainerRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_TrainerNotFound_ReturnsNotFoundWithoutSaving()
    {
        var trainerRepository = new Mock<ITrainerRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(trainerRepository, personRepository, unitOfWork);

        var dto = new TrainerUpdateDto
        {
            Specialization = "Updated",
            IsActive = true,
            Notes = "Updated notes"
        };

        trainerRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                cancellationToken))
            .ReturnsAsync((Trainer?)null);

        var result = await service.UpdateAsync(9, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ServiceErrors.Common.NotFound.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEntityAndSavesChanges()
    {
        var trainerRepository = new Mock<ITrainerRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(trainerRepository, personRepository, unitOfWork);

        var trainer = Trainer.Create(
            Id.Create(7).Value,
            "Old specialization",
            DateTime.UtcNow.AddDays(-30),
            "Old notes").Value;

        var dto = new TrainerUpdateDto
        {
            Specialization = "New specialization",
            IsActive = false,
            Notes = "New notes"
        };

        trainerRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                cancellationToken))
            .ReturnsAsync(trainer);

        var result = await service.UpdateAsync(9, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        trainer.Specialization.Should().Be(dto.Specialization);
        trainer.IsActive.Should().BeFalse();
        trainer.Notes.Should().Be(dto.Notes);
        trainer.UpdatedAt.Should().NotBeNull();

        unitOfWork.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static TrainerService CreateService(
        Mock<ITrainerRepository> trainerRepository,
        Mock<IPersonRepository> personRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new TrainerService(
            trainerRepository.Object,
            personRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<TrainerService>>());
    }
}

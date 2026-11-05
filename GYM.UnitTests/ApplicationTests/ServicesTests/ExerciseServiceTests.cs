using FluentAssertions;
using GYM.Application.DTOs.Exercise;
using GYM.Application.Errors;
using GYM.Application.Services;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class ExerciseServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidGymId_ReturnsValidationFailureWithoutPersisting()
    {
        var exerciseRepository = new Mock<IExerciseRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(exerciseRepository, gymRepository, unitOfWork);

        var dto = new CreateExerciseDto
        {
            GymId = 0,
            Name = "Bench Press",
            MuscleGroup = "Chest",
            Description = "desc",
            Instructions = "instructions",
            VideoUrl = "video"
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.ValueObjects.Id.LessThanOrEqualToZero.Code);

        exerciseRepository.Verify(repository => repository.Add(It.IsAny<Exercise>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_GymNotFoundAndDuplicateName_ReturnsValidationFailureWithoutPersisting()
    {
        var exerciseRepository = new Mock<IExerciseRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(exerciseRepository, gymRepository, unitOfWork);

        var dto = new CreateExerciseDto
        {
            GymId = 2,
            Name = "  Squat  ",
            MuscleGroup = "Legs",
            Description = "desc",
            Instructions = "instructions",
            VideoUrl = "video"
        };

        gymRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.GymId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        exerciseRepository
            .Setup(repository => repository.ExistsByNameAsync("Squat", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Common.NotFound.Code);
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Exercise.Name.AlreadyExists.Code);

        exerciseRepository.Verify(repository => repository.Add(It.IsAny<Exercise>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidDomainData_ReturnsValidationFailureWithoutPersisting()
    {
        var exerciseRepository = new Mock<IExerciseRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(exerciseRepository, gymRepository, unitOfWork);

        var dto = new CreateExerciseDto
        {
            GymId = 2,
            Name = "Deadlift",
            MuscleGroup = "  ",
            Description = "desc",
            Instructions = "instructions",
            VideoUrl = "video"
        };

        gymRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.GymId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        exerciseRepository
            .Setup(repository => repository.ExistsByNameAsync("Deadlift", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Exercise.MuscleGroup.Empty.Code);

        exerciseRepository.Verify(repository => repository.Add(It.IsAny<Exercise>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutSaving()
    {
        var exerciseRepository = new Mock<IExerciseRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(exerciseRepository, gymRepository, unitOfWork);

        var dto = new ExerciseUpdateDto
        {
            Name = "Squat",
            MuscleGroup = "Legs",
            Description = "desc",
            Instructions = "instructions",
            VideoUrl = "video",
            IsActive = true
        };

        var result = await service.UpdateAsync(0, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero.Code);

        exerciseRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExerciseNotFound_ReturnsNotFoundWithoutSaving()
    {
        var exerciseRepository = new Mock<IExerciseRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(exerciseRepository, gymRepository, unitOfWork);

        exerciseRepository
            .Setup(repository => repository.GetByIdAsync(It.Is<Id>(id => id.Value == 15), cancellationToken))
            .ReturnsAsync((Exercise?)null);

        var dto = new ExerciseUpdateDto
        {
            Name = "Squat",
            MuscleGroup = "Legs",
            Description = "desc",
            Instructions = "instructions",
            VideoUrl = "video",
            IsActive = true
        };

        var result = await service.UpdateAsync(15, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ServiceErrors.Common.NotFound.Code);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEntityAndSavesChanges()
    {
        var exerciseRepository = new Mock<IExerciseRepository>();
        var gymRepository = new Mock<IGymRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(exerciseRepository, gymRepository, unitOfWork);

        var exercise = Exercise.Create(
            Id.Create(2).Value,
            "Old Name",
            "Back",
            "old desc",
            "old instructions",
            "https://video.example/old").Value;

        exerciseRepository
            .Setup(repository => repository.GetByIdAsync(It.Is<Id>(id => id.Value == 7), cancellationToken))
            .ReturnsAsync(exercise);

        exerciseRepository
            .Setup(repository => repository.GetByNameAsync("Updated Name", cancellationToken))
            .ReturnsAsync((Exercise?)null);

        var dto = new ExerciseUpdateDto
        {
            Name = "  Updated Name  ",
            MuscleGroup = "  Shoulders  ",
            Description = "  updated desc  ",
            Instructions = "  updated instructions  ",
            VideoUrl = "  https://video.example/updated  ",
            IsActive = false
        };

        var result = await service.UpdateAsync(7, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        exercise.Name.Should().Be("Updated Name");
        exercise.MuscleGroup.Should().Be("Shoulders");
        exercise.Description.Should().Be("updated desc");
        exercise.Instructions.Should().Be("updated instructions");
        exercise.VideoUrl.Should().Be("https://video.example/updated");
        exercise.IsActive.Should().BeFalse();
        exercise.UpdatedAt.Should().NotBeNull();

        unitOfWork.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static ExerciseService CreateService(
        Mock<IExerciseRepository> exerciseRepository,
        Mock<IGymRepository> gymRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new ExerciseService(
            exerciseRepository.Object,
            gymRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<ExerciseService>>());
    }
}

using FluentAssertions;
using GYM.Application;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Person;
using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class PersonServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidPersonFields_ReturnsValidationFailureWithoutPersisting()
    {
        var repository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var dto = new CreatePersonDto
        {
            GymId = 5,
            FullName = "A",
            Phone = "+1234567890",
            Gender = Gender.Female
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle(error => error.Code == "Person.FullName.TooShort");
        repository.Verify(repository => repository.Add(It.IsAny<Person>()), Times.Never);
        unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_AddressTooLong_ReturnsValidationFailureWithoutPersisting()
    {
        var repository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var dto = new CreatePersonDto
        {
            GymId = 5,
            FullName = "Alice Smith",
            Phone = "+1234567890",
            Gender = Gender.Male,
            Address = new string('x', Person.Constants.AddressMaxLength + 1)
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle(error => error.Code == "Person.Address.TooLong");
        repository.Verify(repository => repository.Add(It.IsAny<Person>()), Times.Never);
        unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPerson_UpdatesEntityAndSavesChanges()
    {
        var repository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var person = CreatePerson();
        var cancellationToken = new CancellationTokenSource().Token;

        repository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 12),
                cancellationToken))
            .ReturnsAsync(person);

        var dto = new UpdatePersonDto
        {
            FullName = "Updated Name",
            Phone = "+1987654321",
            Email = "updated@example.com",
            DateOfBirth = new DateOnly(1985, 5, 10),
            Gender = Gender.Female,
            Address = "Updated address"
        };

        var result = await service.UpdateAsync(12, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        person.FullName.Should().Be("Updated Name");
        person.Phone.Value.Should().Be("+1987654321");
        person.Email!.Value.Should().Be("updated@example.com");
        person.DateOfBirth!.Value.Should().Be(new DateOnly(1985, 5, 10));
        person.Gender.Should().Be(Gender.Female);
        person.Address.Should().Be("Updated address");
        repository.Verify(repository => repository.GetByIdAsync(
            It.Is<Id>(id => id.Value == 12),
            cancellationToken), Times.Once);
        unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PersonDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var repository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var cancellationToken = new CancellationTokenSource().Token;
        var dto = new UpdatePersonDto
        {
            FullName = "Name",
            Phone = "+1234567890",
            Gender = Gender.Male
        };

        repository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 12),
                cancellationToken))
            .ReturnsAsync((Person?)null);

        var result = await service.UpdateAsync(12, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Common.NotFound");
        unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidData_ReturnsValidationFailureWithoutSaving()
    {
        var repository = new Mock<IPersonRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);
        var person = CreatePerson();
        var dto = new UpdatePersonDto
        {
            FullName = "A",
            Phone = "invalid",
            Gender = Gender.Male
        };

        repository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 12),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        var result = await service.UpdateAsync(12, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().ContainSingle(error => error.Code == "Person.FullName.TooShort");
        unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static IPersonService CreateService(
        Mock<IPersonRepository> repository,
        Mock<IUnitOfWork> unitOfWork)
    {
        var provider = new ServiceCollection()
            .AddSingleton(repository.Object)
            .AddSingleton(unitOfWork.Object)
            .AddLogging()
            .AddApplication()
            .BuildServiceProvider();

        return provider.GetRequiredService<IPersonService>();
    }

    private static Person CreatePerson()
    {
        var person = Person.Create(
            Id.Create(5).Value,
            "Original Name",
            Phone.Create("+1234567890").Value,
            null,
            null,
            Gender.Male,
            "Original address").Value;

        return person;
    }
}

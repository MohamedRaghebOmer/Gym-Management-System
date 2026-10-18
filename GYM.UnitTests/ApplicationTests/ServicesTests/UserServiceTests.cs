using FluentAssertions;
using GYM.Application.Abstractions;
using GYM.Application.DTOs.User;
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

public sealed class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidIds_ReturnsValidationFailureWithoutPersisting()
    {
        var userRepository = new Mock<IUserRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();

        var service = CreateService(userRepository, personRepository, roleRepository, unitOfWork, passwordHasher);

        var dto = new CreateUserDto
        {
            PersonId = 0,
            Username = "valid.username",
            Password = "valid-password",
            RoleId = 0
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");

        passwordHasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        userRepository.Verify(repository => repository.Add(It.IsAny<User>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PersonAlreadyAssociated_ReturnsValidationFailureWithoutPersisting()
    {
        var userRepository = new Mock<IUserRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();

        var service = CreateService(userRepository, personRepository, roleRepository, unitOfWork, passwordHasher);

        var dto = new CreateUserDto
        {
            PersonId = 5,
            Username = "john.doe",
            Password = "valid-password",
            RoleId = 2
        };

        personRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        roleRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.RoleId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        userRepository
            .Setup(repository => repository.ExistsByPersonIdAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        userRepository
            .Setup(repository => repository.ExistsByUsernameAsync(
                dto.Username,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == ServiceErrors.User.Person.AlreadyAssociatedWithUser.Code);

        passwordHasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        userRepository.Verify(repository => repository.Add(It.IsAny<User>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ReturnsValidationFailureWithoutPersisting()
    {
        var userRepository = new Mock<IUserRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();

        var service = CreateService(userRepository, personRepository, roleRepository, unitOfWork, passwordHasher);

        var dto = new CreateUserDto
        {
            PersonId = 5,
            Username = "john.doe",
            Password = "valid-password",
            RoleId = 2
        };

        personRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        roleRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.RoleId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        userRepository
            .Setup(repository => repository.ExistsByPersonIdAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        userRepository
            .Setup(repository => repository.ExistsByUsernameAsync(
                dto.Username,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == ServiceErrors.User.Username.AlreadyExists.Code);

        passwordHasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        userRepository.Verify(repository => repository.Add(It.IsAny<User>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutUpdating()
    {
        var userRepository = new Mock<IUserRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();

        var service = CreateService(userRepository, personRepository, roleRepository, unitOfWork, passwordHasher);

        var dto = new UpdateUserDto
        {
            Username = "john.updated",
            Password = "new-password",
            IsActive = true
        };

        var result = await service.UpdateAsync(0, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");

        userRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        passwordHasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UserNotFound_ReturnsNotFoundWithoutSaving()
    {
        var userRepository = new Mock<IUserRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(userRepository, personRepository, roleRepository, unitOfWork, passwordHasher);

        var dto = new UpdateUserDto
        {
            Username = "john.updated",
            Password = "new-password",
            IsActive = true
        };

        userRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                cancellationToken))
            .ReturnsAsync((User?)null);

        var result = await service.UpdateAsync(9, dto, cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ServiceErrors.Common.NotFound.Code);

        passwordHasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesEntityAndSavesChanges()
    {
        var userRepository = new Mock<IUserRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(userRepository, personRepository, roleRepository, unitOfWork, passwordHasher);

        var user = User.Create(
            Id.Create(5).Value,
            "john.old",
            "old-hash",
            Id.Create(2).Value).Value;

        var dto = new UpdateUserDto
        {
            Username = "john.new",
            Password = "new-password",
            IsActive = false
        };

        userRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                cancellationToken))
            .ReturnsAsync(user);

        userRepository
            .Setup(repository => repository.GetByUsernameAsync(
                dto.Username,
                cancellationToken))
            .ReturnsAsync((User?)null);

        passwordHasher
            .Setup(hasher => hasher.Hash(dto.Password))
            .Returns("new-hash");

        var result = await service.UpdateAsync(9, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        user.Username.Should().Be(dto.Username);
        user.PasswordHash.Should().Be("new-hash");
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();

        passwordHasher.Verify(hasher => hasher.Hash(dto.Password), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static UserService CreateService(
        Mock<IUserRepository> userRepository,
        Mock<IPersonRepository> personRepository,
        Mock<IRoleRepository> roleRepository,
        Mock<IUnitOfWork> unitOfWork,
        Mock<IPasswordHasher> passwordHasher)
    {
        return new UserService(
            userRepository.Object,
            personRepository.Object,
            roleRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<UserService>>(),
            passwordHasher.Object);
    }
}

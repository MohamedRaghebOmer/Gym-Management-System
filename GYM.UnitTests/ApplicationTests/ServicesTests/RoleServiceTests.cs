using FluentAssertions;
using GYM.Application;
using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.DTOs.Role;
using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GYM.UnitTests.ApplicationTests.ServicesTests;

public sealed class RoleServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidName_ReturnsValidationFailureWithoutPersisting()
    {
        var repository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<GYM.Domain.Abstractions.IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);

        var dto = new CreateRoleDto
        {
            Name = "A",
            Description = "desc"
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        repository.Verify(r => r.Add(It.IsAny<Role>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingRole_UpdatesAndSaves()
    {
        var repository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<GYM.Domain.Abstractions.IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);

        var create = Role.Create("Member", "basic");
        create.IsSuccess.Should().BeTrue();
        var role = create.Value;

        repository
            .Setup(r => r.GetByIdAsync(It.Is<Id>(id => id.Value == 12), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(role);

        var dto = new UpdateRoleDto
        {
            Name = "Member Plus",
            Description = "upgraded"
        };

        var result = await service.UpdateAsync(12, dto);

        result.IsSuccess.Should().BeTrue();
        role.Name.Should().Be(dto.Name);
        role.Description.Should().Be(dto.Description);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFoundWithoutSaving()
    {
        var repository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<GYM.Domain.Abstractions.IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);

        repository
            .Setup(r => r.GetByIdAsync(It.Is<Id>(id => id.Value == 12), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var dto = new UpdateRoleDto
        {
            Name = "Role",
            Description = null
        };

        var result = await service.UpdateAsync(12, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Common.NotFound");
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidData_ReturnsValidationFailureWithoutSaving()
    {
        var repository = new Mock<IRoleRepository>();
        var unitOfWork = new Mock<GYM.Domain.Abstractions.IUnitOfWork>();
        var service = CreateService(repository, unitOfWork);

        var create = Role.Create("Member", "basic");
        create.IsSuccess.Should().BeTrue();
        var role = create.Value;

        repository
            .Setup(r => r.GetByIdAsync(It.Is<Id>(id => id.Value == 12), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(role);

        var dto = new UpdateRoleDto
        {
            Name = "A",
            Description = "x"
        };

        var result = await service.UpdateAsync(12, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationErrors>();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Never);
    }

    private static IRoleService CreateService(
        Mock<IRoleRepository> repository,
        Mock<GYM.Domain.Abstractions.IUnitOfWork> unitOfWork)
    {
        var provider = new ServiceCollection()
            .AddSingleton(repository.Object)
            .AddSingleton(unitOfWork.Object)
            .AddLogging()
            .AddApplication()
            .BuildServiceProvider();

        return provider.GetRequiredService<IRoleService>();
    }
}

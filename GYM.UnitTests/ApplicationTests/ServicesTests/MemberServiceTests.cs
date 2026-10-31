using FluentAssertions;
using GYM.Application.DTOs.Member;
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

public sealed class MemberServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidPersonId_ReturnsValidationFailureWithoutPersisting()
    {
        var memberRepository = new Mock<IMemberRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var trainerRepository = new Mock<ITrainerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(memberRepository, personRepository, trainerRepository, unitOfWork);

        var dto = new CreateMemberDto
        {
            PersonId = 0,
            TrainerId = null,
            EmergencyContactName = "Emergency Contact",
            EmergencyContactPhone = "+1111111111",
            JoinDate = new DateTime(2025, 1, 1),
            Notes = "Notes"
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");

        memberRepository.Verify(repository => repository.Add(It.IsAny<Member>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PersonAlreadyAssociatedWithMember_ReturnsValidationFailureWithoutPersisting()
    {
        var memberRepository = new Mock<IMemberRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var trainerRepository = new Mock<ITrainerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(memberRepository, personRepository, trainerRepository, unitOfWork);

        var dto = new CreateMemberDto
        {
            PersonId = 5,
            TrainerId = null,
            EmergencyContactName = "Emergency Contact",
            EmergencyContactPhone = "+1111111111",
            JoinDate = new DateTime(2025, 1, 1),
            Notes = "Notes"
        };

        personRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        memberRepository
            .Setup(repository => repository.ExistsByPersonIdAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Member.Person.AlreadyAssociatedWithMember.Code);

        memberRepository.Verify(repository => repository.Add(It.IsAny<Member>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TrainerAlreadyAssociatedWithMember_ReturnsValidationFailureWithoutPersisting()
    {
        var memberRepository = new Mock<IMemberRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var trainerRepository = new Mock<ITrainerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(memberRepository, personRepository, trainerRepository, unitOfWork);

        var dto = new CreateMemberDto
        {
            PersonId = 5,
            TrainerId = 7,
            EmergencyContactName = "Emergency Contact",
            EmergencyContactPhone = "+1111111111",
            JoinDate = new DateTime(2025, 1, 1),
            Notes = "Notes"
        };

        personRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        memberRepository
            .Setup(repository => repository.ExistsByPersonIdAsync(
                It.Is<Id>(id => id.Value == dto.PersonId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        trainerRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.TrainerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        memberRepository
            .Setup(repository => repository.ExistsByTrainerIdAsync(
                It.Is<Id>(id => id.Value == dto.TrainerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Member.Trainer.AlreadyAssociatedWithMember.Code);

        memberRepository.Verify(repository => repository.Add(It.IsAny<Member>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutUpdating()
    {
        var memberRepository = new Mock<IMemberRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var trainerRepository = new Mock<ITrainerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(memberRepository, personRepository, trainerRepository, unitOfWork);

        var dto = new MemberUpdateDto
        {
            TrainerId = null,
            EmergencyContactName = "Updated",
            EmergencyContactPhone = "+2222222222",
            IsActive = false,
            Notes = "Updated notes"
        };

        var result = await service.UpdateAsync(0, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");

        memberRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_TrainerDoesNotExist_ReturnsValidationFailureWithoutSaving()
    {
        var memberRepository = new Mock<IMemberRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var trainerRepository = new Mock<ITrainerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(memberRepository, personRepository, trainerRepository, unitOfWork);

        var member = Member.Create(
            Id.Create(5).Value,
            null,
            "Old Contact",
            "+1234567890",
            new DateTime(2025, 1, 1),
            "Old notes").Value;

        var dto = new MemberUpdateDto
        {
            TrainerId = 12,
            EmergencyContactName = "Updated Contact",
            EmergencyContactPhone = "+2222222222",
            IsActive = true,
            Notes = "Updated notes"
        };

        memberRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        trainerRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.TrainerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.UpdateAsync(9, dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Common.NotFound.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesMemberAndSavesChanges()
    {
        var memberRepository = new Mock<IMemberRepository>();
        var personRepository = new Mock<IPersonRepository>();
        var trainerRepository = new Mock<ITrainerRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        var service = CreateService(memberRepository, personRepository, trainerRepository, unitOfWork);

        var member = Member.Create(
            Id.Create(5).Value,
            Id.Create(7).Value,
            "Old Contact",
            "+1234567890",
            new DateTime(2025, 1, 1),
            "Old notes").Value;

        var joinDate = member.JoinDate;

        var dto = new MemberUpdateDto
        {
            TrainerId = null,
            EmergencyContactName = "  Updated Contact  ",
            EmergencyContactPhone = "  +2222222222  ",
            IsActive = false,
            Notes = "  Updated notes  "
        };

        memberRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 9),
                cancellationToken))
            .ReturnsAsync(member);

        var result = await service.UpdateAsync(9, dto, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        member.TrainerId.Should().BeNull();
        member.EmergencyContactName.Should().Be("Updated Contact");
        member.EmergencyContactPhone.Should().Be("+2222222222");
        member.IsActive.Should().BeFalse();
        member.Notes.Should().Be("Updated notes");
        member.JoinDate.Should().Be(joinDate);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private static MemberService CreateService(
        Mock<IMemberRepository> memberRepository,
        Mock<IPersonRepository> personRepository,
        Mock<ITrainerRepository> trainerRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new MemberService(
            memberRepository.Object,
            personRepository.Object,
            trainerRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<MemberService>>());
    }
}

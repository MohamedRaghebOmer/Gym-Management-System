using FluentAssertions;
using GYM.Application.DTOs.Attendance;
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

public sealed class AttendanceServiceTests
{
    [Fact]
    public async Task CreateAsync_InvalidMemberId_ReturnsValidationFailureWithoutPersisting()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var dto = new CreateAttendanceDto
        {
            MemberId = 0,
            CheckIn = DateTime.UtcNow,
            CheckOut = null
        };

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(error => error.Code == "Id.LessThanOrEqualToZero");

        attendanceRepository.Verify(repository => repository.Add(It.IsAny<Attendance>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_MemberWithoutActiveSubscription_ReturnsValidationFailureWithoutPersisting()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var dto = new CreateAttendanceDto
        {
            MemberId = 5,
            CheckIn = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc),
            CheckOut = null
        };

        memberRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        subscriptionRepository
            .Setup(repository => repository.HasActiveSubscriptionAtAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                DateOnly.FromDateTime(dto.CheckIn),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        attendanceRepository
            .Setup(repository => repository.HasOpenAttendanceAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Attendance.Member.NoActiveSubscription.Code);

        attendanceRepository.Verify(repository => repository.Add(It.IsAny<Attendance>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_OpenAttendanceAlreadyExists_ReturnsValidationFailureWithoutPersisting()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var dto = new CreateAttendanceDto
        {
            MemberId = 7,
            CheckIn = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            CheckOut = null
        };

        memberRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        subscriptionRepository
            .Setup(repository => repository.HasActiveSubscriptionAtAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                DateOnly.FromDateTime(dto.CheckIn),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        attendanceRepository
            .Setup(repository => repository.HasOpenAttendanceAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Attendance.Member.OpenAttendanceAlreadyExists.Code);

        attendanceRepository.Verify(repository => repository.Add(It.IsAny<Attendance>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsAttendanceAndReturnsId()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var dto = new CreateAttendanceDto
        {
            MemberId = 3,
            CheckIn = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        memberRepository
            .Setup(repository => repository.ExistsAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        subscriptionRepository
            .Setup(repository => repository.HasActiveSubscriptionAtAsync(
                It.Is<Id>(id => id.Value == dto.MemberId),
                DateOnly.FromDateTime(dto.CheckIn),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Attendance? addedEntity = null;
        attendanceRepository
            .Setup(repository => repository.Add(It.IsAny<Attendance>()))
            .Callback<Attendance>(entity =>
            {
                addedEntity = entity;
                typeof(Attendance)
                    .BaseType!
                    .GetProperty(nameof(Attendance.Id))!
                    .SetValue(entity, Id.FromDatabase(44));
            });

        var result = await service.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(44);

        addedEntity.Should().NotBeNull();
        addedEntity.MemberId.Value.Should().Be(dto.MemberId);
        addedEntity.CheckIn.Should().Be(dto.CheckIn);
        addedEntity.CheckOut.Should().Be(dto.CheckOut);

        attendanceRepository.Verify(repository => repository.HasOpenAttendanceAsync(
            It.IsAny<Id>(),
            It.IsAny<Id?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailureWithoutSaving()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var result = await service.UpdateAsync(
            0,
            new AttendanceUpdateDto
            {
                CheckIn = DateTime.UtcNow,
                CheckOut = null
            });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");

        attendanceRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Id>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AttendanceNotFound_ReturnsNotFound()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        attendanceRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 8),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        var result = await service.UpdateAsync(
            8,
            new AttendanceUpdateDto
            {
                CheckIn = DateTime.UtcNow,
                CheckOut = null
            });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ServiceErrors.Common.NotFound.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MemberWithoutActiveSubscription_ReturnsValidationFailureWithoutSaving()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var attendance = Attendance.Create(
            Id.Create(5).Value,
            new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
            null).Value;

        typeof(Attendance)
            .BaseType!
            .GetProperty(nameof(Attendance.Id))!
            .SetValue(attendance, Id.FromDatabase(21));

        var dto = new AttendanceUpdateDto
        {
            CheckIn = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc),
            CheckOut = null
        };

        attendanceRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 21),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        subscriptionRepository
            .Setup(repository => repository.HasActiveSubscriptionAtAsync(
                attendance.MemberId,
                DateOnly.FromDateTime(dto.CheckIn),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        attendanceRepository
            .Setup(repository => repository.HasOpenAttendanceAsync(
                attendance.MemberId,
                attendance.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.UpdateAsync(21, dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Attendance.Member.NoActiveSubscription.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OpenAttendanceAlreadyExists_ReturnsValidationFailureWithoutSaving()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var attendance = Attendance.Create(
            Id.Create(6).Value,
            new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
            null).Value;

        typeof(Attendance)
            .BaseType!
            .GetProperty(nameof(Attendance.Id))!
            .SetValue(attendance, Id.FromDatabase(22));

        var dto = new AttendanceUpdateDto
        {
            CheckIn = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc),
            CheckOut = null
        };

        attendanceRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 22),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        subscriptionRepository
            .Setup(repository => repository.HasActiveSubscriptionAtAsync(
                attendance.MemberId,
                DateOnly.FromDateTime(dto.CheckIn),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        attendanceRepository
            .Setup(repository => repository.HasOpenAttendanceAsync(
                attendance.MemberId,
                attendance.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.UpdateAsync(22, dto);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == ServiceErrors.Attendance.Member.OpenAttendanceAlreadyExists.Code);

        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesAttendanceAndSavesChanges()
    {
        var attendanceRepository = new Mock<IAttendanceRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(attendanceRepository, memberRepository, subscriptionRepository, unitOfWork);

        var attendance = Attendance.Create(
            Id.Create(9).Value,
            new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc),
            null).Value;

        attendanceRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Id>(id => id.Value == 30),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        var dto = new AttendanceUpdateDto
        {
            CheckIn = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 10, 1, 11, 0, 0, DateTimeKind.Utc)
        };

        subscriptionRepository
            .Setup(repository => repository.HasActiveSubscriptionAtAsync(
                attendance.MemberId,
                DateOnly.FromDateTime(dto.CheckIn),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.UpdateAsync(30, dto);

        result.IsSuccess.Should().BeTrue();
        attendance.CheckIn.Should().Be(dto.CheckIn);
        attendance.CheckOut.Should().Be(dto.CheckOut);

        attendanceRepository.Verify(repository => repository.HasOpenAttendanceAsync(
            It.IsAny<Id>(),
            It.IsAny<Id?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AttendanceService CreateService(
        Mock<IAttendanceRepository> attendanceRepository,
        Mock<IMemberRepository> memberRepository,
        Mock<ISubscriptionRepository> subscriptionRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new AttendanceService(
            attendanceRepository.Object,
            memberRepository.Object,
            subscriptionRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<AttendanceService>>());
    }
}

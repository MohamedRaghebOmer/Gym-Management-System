using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class AttendanceTests
{
    [Fact]
    public void Create_ValidInput_ReturnsAttendanceWithExpectedState()
    {
        var memberId = Id.Create(5).Value;
        var checkIn = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        var result = Attendance.Create(memberId, checkIn, checkOut);

        result.IsSuccess.Should().BeTrue();
        result.Value.MemberId.Should().Be(memberId);
        result.Value.CheckIn.Should().Be(checkIn);
        result.Value.CheckOut.Should().Be(checkOut);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_DefaultCheckIn_ReturnsValidationFailure()
    {
        var result = Attendance.Create(
            Id.Create(1).Value,
            default,
            DateTime.UtcNow.AddHours(1));

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Attendance.CheckIn.Required.Code);
    }

    [Fact]
    public void Create_CheckOutLessThanOrEqualToCheckIn_ReturnsValidationFailure()
    {
        var checkIn = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc);

        var result = Attendance.Create(
            Id.Create(1).Value,
            checkIn,
            checkIn);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Attendance.CheckOut.MustBeGreaterThanCheckIn.Code);
    }

    [Fact]
    public void Update_ValidInput_UpdatesAttendance()
    {
        var attendance = Attendance.Create(
            Id.Create(3).Value,
            new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            null).Value;

        var newCheckIn = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc);
        var newCheckOut = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = attendance.Update(newCheckIn, newCheckOut);

        result.IsSuccess.Should().BeTrue();
        attendance.CheckIn.Should().Be(newCheckIn);
        attendance.CheckOut.Should().Be(newCheckOut);
    }

    [Fact]
    public void Update_CheckOutLessThanOrEqualToCheckIn_ReturnsValidationFailureWithoutChangingState()
    {
        var originalCheckIn = new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc);
        var originalCheckOut = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(
            Id.Create(3).Value,
            originalCheckIn,
            originalCheckOut).Value;

        var result = attendance.Update(
            new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc));

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Attendance.CheckOut.MustBeGreaterThanCheckIn.Code);

        attendance.CheckIn.Should().Be(originalCheckIn);
        attendance.CheckOut.Should().Be(originalCheckOut);
    }
}

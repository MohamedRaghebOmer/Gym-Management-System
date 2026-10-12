using FluentAssertions;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.ValueObjectsTests;

public sealed class DateOfBirthTests
{
    [Fact]
    public void Create_ValidDateOfBirth_ReturnsDateOfBirth()
    {
        var value = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30));

        var result = DateOfBirth.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Fact]
    public void Create_FutureDate_ReturnsFailure()
    {
        var value = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var result = DateOfBirth.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DateOfBirth.InFuture");
    }

    [Fact]
    public void Create_TooOldDate_ReturnsFailure()
    {
        var value = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-121));

        var result = DateOfBirth.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DateOfBirth.TooOld");
    }
}

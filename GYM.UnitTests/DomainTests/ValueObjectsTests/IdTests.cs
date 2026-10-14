using FluentAssertions;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.ValueObjectsTests;

public sealed class IdTests
{
    [Fact]
    public void Create_PositiveValue_ReturnsId()
    {
        var result = Id.Create(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(42);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_NonPositiveValue_ReturnsFailure(int value)
    {
        var result = Id.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Id.LessThanOrEqualToZero");
    }
}

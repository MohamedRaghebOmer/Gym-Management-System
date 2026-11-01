using FluentAssertions;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.ValueObjectsTests;

public sealed class MoneyTests
{
    [Fact]
    public void Create_PositiveValue_ReturnsMoney()
    {
        var result = Money.Create(50.75m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(50.75m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_NonPositiveValue_ReturnsFailure(decimal value)
    {
        var result = Money.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.LessThanOrEqualToZero");
    }
}

using FluentAssertions;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.ValueObjectsTests;

public sealed class PhoneTests
{
    [Fact]
    public void Create_ValidPhone_ReturnsPhone()
    {
        var result = Phone.Create("+1234567890");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890")]
    public void Create_InvalidPhoneFormat_ReturnsFailure(string value)
    {
        var result = Phone.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().BeOneOf("Phone.Empty", "Phone.InvalidFormat");
    }

    [Fact]
    public void Create_TooShortPhone_ReturnsFailure()
    {
        var result = Phone.Create("+123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.InvalidLength");
    }
}

using FluentAssertions;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.ValueObjectsTests;

public sealed class EmailTests
{
    [Fact]
    public void Create_ValidEmail_ReturnsEmail()
    {
        var result = Email.Create("hello@example.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("hello@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_InvalidEmail_ReturnsFailure(string value)
    {
        var result = Email.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().BeOneOf("Email.Empty", "Email.InvalidFormat");
    }

    [Fact]
    public void Create_EmailLongerThanMaximum_ReturnsFailure()
    {
        var value = new string('a', 321) + "@example.com";

        var result = Email.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.TooLong");
    }
}

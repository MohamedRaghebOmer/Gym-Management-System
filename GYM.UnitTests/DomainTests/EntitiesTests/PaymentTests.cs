using FluentAssertions;
using GYM.Domain.Entities;
using GYM.Domain.Enums;
using GYM.Domain.Errors;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.UnitTests.DomainTests.EntitiesTests;

public sealed class PaymentTests
{
    [Fact]
    public void Create_ValidInput_ReturnsPaymentWithExpectedState()
    {
        var subscriptionId = Id.Create(10).Value;
        var amount = Money.Create(120.50m).Value;
        var paymentDate = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc);

        var result = Payment.Create(
            subscriptionId,
            amount,
            PaymentMethod.Card,
            paymentDate,
            "  first payment  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.SubscriptionId.Should().Be(subscriptionId);
        result.Value.Amount.Should().Be(amount);
        result.Value.PaymentMethod.Should().Be(PaymentMethod.Card);
        result.Value.PaymentDate.Should().Be(paymentDate);
        result.Value.Notes.Should().Be("first payment");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_InvalidPaymentMethod_ReturnsValidationFailure()
    {
        var result = Payment.Create(
            Id.Create(1).Value,
            Money.Create(100m).Value,
            (PaymentMethod)99,
            DateTime.UtcNow,
            "note");

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Payment.PaymentMethod.Invalid.Code);
    }

    [Fact]
    public void Create_DefaultPaymentDate_ReturnsValidationFailure()
    {
        var result = Payment.Create(
            Id.Create(1).Value,
            Money.Create(100m).Value,
            PaymentMethod.Cash,
            default,
            "note");

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Payment.PaymentDate.Required.Code);
    }

    [Fact]
    public void Create_NotesTooLong_ReturnsValidationFailure()
    {
        var tooLongNotes = new string('x', Payment.Constants.NotesMaxLength + 1);

        var result = Payment.Create(
            Id.Create(1).Value,
            Money.Create(100m).Value,
            PaymentMethod.BankTransfer,
            DateTime.UtcNow,
            tooLongNotes);

        result.IsFailure.Should().BeTrue();
        var validationErrors = result.Error.Should().BeOfType<ValidationErrors>().Subject;
        validationErrors.Errors.Should().Contain(
            error => error.Code == DomainErrors.Entities.Payment.Notes.TooLong.Code);
    }

    [Fact]
    public void Update_ValidNotes_UpdatesPaymentNotes()
    {
        var payment = Payment.Create(
            Id.Create(3).Value,
            Money.Create(60m).Value,
            PaymentMethod.Cash,
            DateTime.UtcNow,
            "old note").Value;

        var result = payment.Update("  updated note  ");

        result.IsSuccess.Should().BeTrue();
        payment.Notes.Should().Be("updated note");
    }

    [Fact]
    public void Update_NotesTooLong_ReturnsFailureWithoutChangingState()
    {
        var payment = Payment.Create(
            Id.Create(3).Value,
            Money.Create(60m).Value,
            PaymentMethod.Cash,
            DateTime.UtcNow,
            "original").Value;

        var result = payment.Update(new string('x', Payment.Constants.NotesMaxLength + 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.Entities.Payment.Notes.TooLong.Code);
        payment.Notes.Should().Be("original");
    }
}

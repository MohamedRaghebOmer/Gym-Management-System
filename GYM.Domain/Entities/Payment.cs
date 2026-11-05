using GYM.Domain.Enums;
using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Payment : Entity
{
    public static class Constants
    {
        public const int NotesMaxLength = 500;
    }

    private Payment(
        Id subscriptionId,
        Money amount,
        PaymentMethod paymentMethod,
        DateTime paymentDate,
        string? notes)
    {
        SubscriptionId = subscriptionId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        PaymentDate = paymentDate;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    private Payment() { } // For EF Core

    public Id SubscriptionId { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private init; }

    public Subscription? Subscription { get; private set; }

    public static Result<Payment> Create(
        Id subscriptionId,
        Money amount,
        PaymentMethod paymentMethod,
        DateTime paymentDate,
        string? notes)
    {
        notes = notes?.Trim();

        var result = Validate(paymentMethod, paymentDate, notes);
        if (result.IsFailure)
            return Result.Failure<Payment>(result.Error);

        return new Payment(subscriptionId, amount, paymentMethod, paymentDate, notes);
    }

    public Result Update(
        string? notes)
    {
        notes = notes?.Trim();

        if (notes?.Length > Constants.NotesMaxLength)
            return Result.Failure(DomainErrors.Entities.Payment.Notes.TooLong);

        Notes = notes;

        return Result.Success();
    }

    private static Result Validate(
        PaymentMethod paymentMethod,
        DateTime paymentDate,
        string? notes)
    {
        List<Error> errors = [];

        if (!Enum.IsDefined(paymentMethod))
            errors.Add(DomainErrors.Entities.Payment.PaymentMethod.Invalid);

        if (paymentDate == default)
            errors.Add(DomainErrors.Entities.Payment.PaymentDate.Required);

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > Constants.NotesMaxLength)
            errors.Add(DomainErrors.Entities.Payment.Notes.TooLong);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }
}

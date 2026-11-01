using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;

namespace GYM.Domain.ValueObjects;

public sealed record Money : ValueObject
{
    public decimal Value { get; private set; }

    private Money(decimal value)
    {
        Value = value;
    }

    public static Result<Money> Create(decimal value)
    {
        if (value <= 0)
            return Result.Failure<Money>(DomainErrors.ValueObjects.Money.LessThanOrEqualToZero);

        return new Money(value);
    }

    public static Money FromDatabase(decimal value)
        => new(value);
}

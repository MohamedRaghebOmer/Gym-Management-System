using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;

namespace GYM.Domain.ValueObjects;

public sealed record Id : ValueObject
{
    public int Value { get; private set; }

    private Id(int value)
    {
        Value = value;
    }

    public static Result<Id> Create(int value)
    {
        if (value <= 0)
        {
            return Result.Failure<Id>(DomainErrors.ValueObjects.Id.LessThanOrEqualToZero);
        }

        return new Id(value);
    }


    public static Id FromDatabase(int value)
        => new Id(value);
}
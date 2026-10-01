using GYM.Domain.Shared;

namespace GYM.Domain.Errors;

public static class DomainErrors
{
    public static class ValueObjects
    {
        public static class Id
        {
            public static readonly Error LessThanOrEqualToZero = new Error("Id.LessThanOrEqualToZero", "Id must be greater than zero.");
        }
    }
}
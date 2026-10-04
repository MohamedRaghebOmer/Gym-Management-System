using GYM.Domain.Shared;

namespace GYM.Domain.Errors;

public static class DomainErrors
{
    public static class ValueObjects
    {
        public static class Id
        {
            public static readonly Error LessThanOrEqualToZero = new(
                "Id.LessThanOrEqualToZero",
                "Id must be greater than zero.");
        }

        public static class Phone
        {
            public static readonly Error Empty = new(
                "Phone.Empty",
                "Phone number cannot be empty.");

            public static readonly Error InvalidLength = new(
                "Phone.InvalidLength",
                $"Phone number must be between {Domain.ValueObjects.Phone.MinLength} and {Domain.ValueObjects.Phone.MaxLength} characters.");

            public static readonly Error InvalidFormat = new(
                "Phone.InvalidFormat",
                "Phone number must be in the format +[country code][number].");
        }

        public static class Email
        {
            public static readonly Error Empty = new(
                "Email.Empty",
                "Email cannot be empty.");

            public static readonly Error InvalidFormat = new(
                "Email.InvalidFormat",
                "Email must be in a valid format.");

            public static readonly Error TooLong = new(
                "Email.TooLong",
                $"Email cannot be longer than {Domain.ValueObjects.Email.MaxLength} characters."
            );

        }

        public static class DateOfBirth
        {
            public static readonly Error InFuture = new(
                "DateOfBirth.InFuture",
                "Date of birth cannot be in the future.");

            public static readonly Error TooOld = new(
                "DateOfBirth.TooOld",
                $"Date of birth cannot be more than {Domain.ValueObjects.DateOfBirth.MaxAge} years ago.");
        }
    }

    public static class Entities
    {
        public static class Person
        {
            public static class FullName
            {
                public static readonly Error Empty = new(
                    "Person.FullName.Empty",
                    "Full name cannot be empty.");

                public static readonly Error TooShort = new(
                    "Person.FullName.TooShort",
                    $"Full name must be at least {Domain.Entities.Person.Constants.FullNameMinLength} characters long.");

                public static readonly Error TooLong = new(
                    "Person.FullName.TooLong",
                    $"Full name cannot be longer than {Domain.Entities.Person.Constants.FullNameMaxLength} characters.");
            }

            public static class Address
            {
                public static readonly Error TooLong = new(
                    "Person.Address.TooLong",
                    $"Address cannot be longer than {Domain.Entities.Person.Constants.AddressMaxLength} characters.");
            }
        }
    }
}
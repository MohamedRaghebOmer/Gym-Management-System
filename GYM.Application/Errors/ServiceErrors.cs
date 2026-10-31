using GYM.Domain.Shared;

namespace GYM.Application.Errors;

public static class ServiceErrors
{
    public static class Common
    {
        public static readonly Error UnExcepected = new(
            "Common.Unexpected",
            "An unexpected error occurred.");

        public static readonly Error NotFound = new(
            "Common.NotFound",
            "The requested resource was not found.");
    }

    public static class Trainer
    {
        public static class Person
        {
            public static readonly Error AlreadyAssociatedWithTrainer = new(
                "Trainer.Person.AlreadyAssociatedWithTrainer",
                "The person is already associated with a trainer.");
        }
    }

    public static class Member
    {
        public static class Person
        {
            public static readonly Error AlreadyAssociatedWithMember = new(
                "Member.Person.AlreadyAssociatedWithMember",
                "The person is already associated with a member.");
        }

        public static class Trainer
        {
            public static readonly Error AlreadyAssociatedWithMember = new(
                "Member.Trainer.AlreadyAssociatedWithMember",
                "The trainer is already associated with a member.");
        }
    }

    public static class User
    {
        public static class Person
        {
            public static readonly Error AlreadyAssociatedWithUser = new(
                "User.Person.AlreadyAssociatedWithUser",
                "The person is already associated with a user.");
        }

        public static class Username
        {
            public static readonly Error AlreadyExists = new(
                "User.Username.AlreadyExists",
                "Username already exists.");
        }
    }
}

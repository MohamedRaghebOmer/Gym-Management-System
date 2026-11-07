using GYM.Domain.Shared;

namespace GYM.Application.Errors;

public static class ServiceErrors
{
    public static class Common
    {
        public static readonly Error Unexpected = new(
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

    public static class MembershipPlan
    {
        public static class Name
        {
            public static readonly Error AlreadyExists = new(
                "MembershipPlan.Name.AlreadyExists",
                "Membership plan name already exists.");
        }
    }

    public static class Subscription
    {
        public static class Member
        {
            public static readonly Error OverlappingActiveSubscription = new(
                "Subscription.Member.OverlappingActiveSubscription",
                "The member already has an overlapping active subscription.");
        }
    }

    public static class Exercise
    {
        public static class Name
        {
            public static readonly Error AlreadyExists = new(
                "Exercise.Name.AlreadyExists",
                "Exercise name already exists.");
        }
    }

    public static class Attendance
    {
        public static class Member
        {
            public static readonly Error NoActiveSubscription = new(
                "Attendance.Member.NoActiveSubscription",
                "The member must have an active subscription to check in.");

            public static readonly Error OpenAttendanceAlreadyExists = new(
                "Attendance.Member.OpenAttendanceAlreadyExists",
                "The member already has an open attendance record.");
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

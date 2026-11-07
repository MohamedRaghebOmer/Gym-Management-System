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

        public static class Money
        {
            public static readonly Error LessThanOrEqualToZero = new(
                "Money.LessThanOrEqualToZero",
                "Money value must be greater than zero.");
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

        public static class Role
        {
            public static class Name
            {
                public static readonly Error Empty = new(
                    "Role.Name.Empty",
                    "Role name cannot be empty.");

                public static readonly Error TooShort = new(
                    "Role.Name.TooShort",
                    $"Role name must be at least {Domain.Entities.Role.Constants.NameMinLength} characters long.");

                public static readonly Error TooLong = new(
                    "Role.Name.TooLong",
                    $"Role name cannot be longer than {Domain.Entities.Role.Constants.NameMaxLength} characters.");
            }

            public static class Description
            {
                public static readonly Error TooLong = new(
                    "Role.Description.TooLong",
                    $"Role description cannot be longer than {Domain.Entities.Role.Constants.DescriptionMaxLength} characters.");
            }
        }

        public static class Gym
        {
            public static class Name
            {
                public static readonly Error Empty = new(
                    "Gym.Name.Empty",
                    "Gym name cannot be empty.");

                public static readonly Error TooShort = new(
                    "Gym.Name.TooShort",
                    $"Gym name must be at least {Domain.Entities.Gym.Constants.NameMinLength} characters long.");

                public static readonly Error TooLong = new(
                    "Gym.Name.TooLong",
                    $"Gym name cannot be longer than {Domain.Entities.Gym.Constants.NameMaxLength} characters.");
            }
        }

        public static class Trainer
        {
            public static class HireDate
            {
                public static readonly Error Required = new(
                    "Trainer.HireDate.Required",
                    "Hire date is required.");

                public static readonly Error InTheFuture = new(
                    "Trainer.HireDate.InTheFuture",
                    "Hire date cannot be in the future.");
            }

            public static class Specialization
            {
                public static readonly Error TooLong = new(
                    "Trainer.Specialization.TooLong",
                    $"Specialization cannot be longer than {Domain.Entities.Trainer.Constants.SpecializationMaxLength} characters.");
            }

            public static class Notes
            {
                public static readonly Error TooLong = new(
                    "Trainer.Notes.TooLong",
                    $"Notes cannot be longer than {Domain.Entities.Trainer.Constants.NotesMaxLength} characters.");
            }
        }

        public static class Member
        {
            public static class JoinDate
            {
                public static readonly Error Required = new(
                    "Member.JoinDate.Required",
                    "Join date is required.");

                public static readonly Error InTheFuture = new(
                    "Member.JoinDate.InTheFuture",
                    "Join date cannot be in the future.");
            }

            public static class EmergencyContactName
            {
                public static readonly Error TooLong = new(
                    "Member.EmergencyContactName.TooLong",
                    $"Emergency contact name cannot be longer than {Domain.Entities.Member.Constants.EmergencyContactNameMaxLength} characters.");
            }

            public static class EmergencyContactPhone
            {
                public static readonly Error TooLong = new(
                    "Member.EmergencyContactPhone.TooLong",
                    $"Emergency contact phone cannot be longer than {Domain.Entities.Member.Constants.EmergencyContactPhoneMaxLength} characters.");
            }

            public static class Notes
            {
                public static readonly Error TooLong = new(
                    "Member.Notes.TooLong",
                    $"Notes cannot be longer than {Domain.Entities.Member.Constants.NotesMaxLength} characters.");
            }
        }

        public static class MembershipPlan
        {
            public static class Name
            {
                public static readonly Error Empty = new(
                    "MembershipPlan.Name.Empty",
                    "Membership plan name cannot be empty.");

                public static readonly Error TooLong = new(
                    "MembershipPlan.Name.TooLong",
                    $"Membership plan name cannot be longer than {Domain.Entities.MembershipPlan.Constants.NameMaxLength} characters.");
            }

            public static class DurationInDays
            {
                public static readonly Error LessThanOrEqualToZero = new(
                    "MembershipPlan.DurationInDays.LessThanOrEqualToZero",
                    "Membership plan duration must be greater than zero.");
            }

            public static class Description
            {
                public static readonly Error TooLong = new(
                    "MembershipPlan.Description.TooLong",
                    $"Membership plan description cannot be longer than {Domain.Entities.MembershipPlan.Constants.DescriptionMaxLength} characters.");
            }
        }

        public static class Exercise
        {
            public static class Name
            {
                public static readonly Error Empty = new(
                    "Exercise.Name.Empty",
                    "Exercise name cannot be empty.");

                public static readonly Error TooLong = new(
                    "Exercise.Name.TooLong",
                    $"Exercise name cannot be longer than {Domain.Entities.Exercise.Constants.NameMaxLength} characters.");
            }

            public static class MuscleGroup
            {
                public static readonly Error Empty = new(
                    "Exercise.MuscleGroup.Empty",
                    "Exercise muscle group cannot be empty.");

                public static readonly Error TooLong = new(
                    "Exercise.MuscleGroup.TooLong",
                    $"Exercise muscle group cannot be longer than {Domain.Entities.Exercise.Constants.MuscleGroupMaxLength} characters.");
            }

            public static class Description
            {
                public static readonly Error TooLong = new(
                    "Exercise.Description.TooLong",
                    $"Exercise description cannot be longer than {Domain.Entities.Exercise.Constants.DescriptionMaxLength} characters.");
            }

            public static class Instructions
            {
                public static readonly Error TooLong = new(
                    "Exercise.Instructions.TooLong",
                    $"Exercise instructions cannot be longer than {Domain.Entities.Exercise.Constants.InstructionsMaxLength} characters.");
            }

            public static class VideoUrl
            {
                public static readonly Error TooLong = new(
                    "Exercise.VideoUrl.TooLong",
                    $"Exercise video url cannot be longer than {Domain.Entities.Exercise.Constants.VideoUrlMaxLength} characters.");

                public static readonly Error InvalidFormat = new(
                    "Exercise.VideoUrl.InvalidFormat",
                    "Exercise video url must be a valid absolute HTTP or HTTPS URL.");
            }
        }

        public static class Subscription
        {
            public static class StartDate
            {
                public static readonly Error Required = new(
                    "Subscription.StartDate.Required",
                    "Subscription start date is required.");
            }

            public static class EndDate
            {
                public static readonly Error Required = new(
                    "Subscription.EndDate.Required",
                    "Subscription end date is required.");

                public static readonly Error MustBeGreaterThanStartDate = new(
                    "Subscription.EndDate.MustBeGreaterThanStartDate",
                    "Subscription end date must be greater than start date.");
            }
        }

        public static class Payment
        {
            public static class PaymentMethod
            {
                public static readonly Error Invalid = new(
                    "Payment.PaymentMethod.Invalid",
                    "Payment method is invalid.");
            }

            public static class PaymentDate
            {
                public static readonly Error Required = new(
                    "Payment.PaymentDate.Required",
                    "Payment date is required.");
            }

            public static class Notes
            {
                public static readonly Error TooLong = new(
                    "Payment.Notes.TooLong",
                    $"Payment notes cannot be longer than {Domain.Entities.Payment.Constants.NotesMaxLength} characters.");
            }
        }

        public static class Attendance
        {
            public static class CheckIn
            {
                public static readonly Error Required = new(
                    "Attendance.CheckIn.Required",
                    "Check-in time is required.");
            }

            public static class CheckOut
            {
                public static readonly Error MustBeGreaterThanCheckIn = new(
                    "Attendance.CheckOut.MustBeGreaterThanCheckIn",
                    "Check-out time must be greater than check-in time.");
            }
        }

        public static class User
        {
            public static class Username
            {
                public static readonly Error Empty = new(
                    "User.Username.Empty",
                    "Username cannot be empty.");

                public static readonly Error TooShort = new(
                    "User.Username.TooShort",
                    $"Username must be at least {Domain.Entities.User.Constants.UsernameMinLength} characters long.");

                public static readonly Error TooLong = new(
                    "User.Username.TooLong",
                    $"Username cannot be longer than {Domain.Entities.User.Constants.UsernameMaxLength} characters.");
            }

            public static class Password
            {
                public static readonly Error Empty = new(
                    "User.Password.Empty",
                    "Password cannot be empty.");

                public static readonly Error TooLong = new(
                    "User.Password.TooLong",
                    $"Password cannot be longer than {Domain.Entities.User.Constants.PasswordHashMaxLength} characters.");
            }
        }

        public static class WorkoutPlan
        {
            public static class Name
            {
                public static readonly Error Required = new(
                    "WorkoutPlan.Name.Required",
                    "Workout plan name is required.");
                public static readonly Error TooLong = new(
                    "WorkoutPlan.Name.TooLong",
                    $"Workout plan name cannot be longer than {Domain.Entities.WorkoutPlan.Constants.NameMaxLength} characters.");
            }
            public static class Description
            {
                public static readonly Error TooLong = new(
                    "WorkoutPlan.Description.TooLong",
                    $"Workout plan description cannot be longer than {Domain.Entities.WorkoutPlan.Constants.DescriptionMaxLength} characters.");
            }

            public static class StartDate
            {
                public static readonly Error Required = new(
                    "WorkoutPlan.StartDate.Required",
                    "Workout plan start date is required.");
            }


            public static class EndDate
            {
                public static readonly Error BeforeStartDate = new(
                    "WorkoutPlan.EndDate.BeforeStartDate",
                    "Workout plan end date must be after start date.");
            }
        }
    }
}

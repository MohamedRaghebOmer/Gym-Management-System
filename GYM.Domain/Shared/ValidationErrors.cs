namespace GYM.Domain.Shared;

public sealed record ValidationErrors : Error
{
    public ValidationErrors(Error[] errors)
        : base(
            "ValidationError",
            "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public Error[] Errors { get; }
}
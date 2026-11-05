using GYM.Domain.Errors;
using GYM.Domain.Primitives;
using GYM.Domain.Shared;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Entities;

public sealed class Exercise : Entity
{
    public static class Constants
    {
        public const int NameMaxLength = 150;
        public const int MuscleGroupMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int InstructionsMaxLength = 2000;
        public const int VideoUrlMaxLength = 500;
    }

    private Exercise(
        Id gymId,
        string name,
        string muscleGroup,
        string? description,
        string? instructions,
        string? videoUrl)
    {
        GymId = gymId;
        Name = name;
        MuscleGroup = muscleGroup;
        Description = description;
        Instructions = instructions;
        VideoUrl = videoUrl;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    private Exercise() { } // For EF Core

    public Id GymId { get; private set; } = default!;
    public string Name { get; private set; } = null!;
    public string MuscleGroup { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Instructions { get; private set; }
    public string? VideoUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }

    public Gym? Gym { get; private set; }

    public static Result<Exercise> Create(
        Id gymId,
        string name,
        string muscleGroup,
        string? description,
        string? instructions,
        string? videoUrl)
    {
        name = name.Trim();
        muscleGroup = muscleGroup.Trim();
        description = description?.Trim();
        instructions = instructions?.Trim();
        videoUrl = videoUrl?.Trim();

        var result = Validate(name, muscleGroup, description, instructions, videoUrl);
        if (result.IsFailure)
            return Result.Failure<Exercise>(result.Error);

        return new Exercise(gymId, name, muscleGroup, description, instructions, videoUrl);
    }

    public Result Update(
        string name,
        string muscleGroup,
        string? description,
        string? instructions,
        string? videoUrl,
        bool isActive)
    {
        name = name.Trim();
        muscleGroup = muscleGroup.Trim();
        description = description?.Trim();
        instructions = instructions?.Trim();
        videoUrl = videoUrl?.Trim();

        var result = Validate(name, muscleGroup, description, instructions, videoUrl);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Name = name;
        MuscleGroup = muscleGroup;
        Description = description;
        Instructions = instructions;
        VideoUrl = videoUrl;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result Validate(
        string name,
        string muscleGroup,
        string? description,
        string? instructions,
        string? videoUrl)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(DomainErrors.Entities.Exercise.Name.Empty);

        if (name.Length > Constants.NameMaxLength)
            errors.Add(DomainErrors.Entities.Exercise.Name.TooLong);

        if (string.IsNullOrWhiteSpace(muscleGroup))
            errors.Add(DomainErrors.Entities.Exercise.MuscleGroup.Empty);

        if (muscleGroup.Length > Constants.MuscleGroupMaxLength)
            errors.Add(DomainErrors.Entities.Exercise.MuscleGroup.TooLong);

        if (!string.IsNullOrWhiteSpace(description)
            && description.Length > Constants.DescriptionMaxLength)
            errors.Add(DomainErrors.Entities.Exercise.Description.TooLong);

        if (!string.IsNullOrWhiteSpace(instructions)
            && instructions.Length > Constants.InstructionsMaxLength)
            errors.Add(DomainErrors.Entities.Exercise.Instructions.TooLong);

        if (!string.IsNullOrWhiteSpace(videoUrl)
            && videoUrl.Length > Constants.VideoUrlMaxLength)
            errors.Add(DomainErrors.Entities.Exercise.VideoUrl.TooLong);

        if (!string.IsNullOrWhiteSpace(videoUrl)
            && !IsValidVideoUrl(videoUrl))
            errors.Add(DomainErrors.Entities.Exercise.VideoUrl.InvalidFormat);

        if (errors.Count > 0)
            return Result.Failure(new ValidationErrors(errors.ToArray()));

        return Result.Success();
    }

    private static bool IsValidVideoUrl(string videoUrl)
    {
        if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp
            || uri.Scheme == Uri.UriSchemeHttps;
    }
}

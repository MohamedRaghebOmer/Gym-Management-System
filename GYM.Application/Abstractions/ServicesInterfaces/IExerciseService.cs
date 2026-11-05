using GYM.Application.DTOs.Exercise;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IExerciseService :
    IService<Exercise, ExerciseResponseDto, CreateExerciseDto, ExerciseUpdateDto>
{
}

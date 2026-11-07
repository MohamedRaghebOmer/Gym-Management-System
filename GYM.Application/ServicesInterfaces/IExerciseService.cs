using GYM.Application.Abstractions;
using GYM.Application.DTOs.Exercise;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IExerciseService :
    IService<Exercise, ExerciseResponseDto, CreateExerciseDto, ExerciseUpdateDto>
{
}

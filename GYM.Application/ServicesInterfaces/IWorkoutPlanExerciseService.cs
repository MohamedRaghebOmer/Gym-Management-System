using GYM.Application.Abstractions;
using GYM.Application.DTOs.WorkoutPlanExercise;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IWorkoutPlanExerciseService
    : IService<WorkoutPlanExercise, WorkoutPlanExerciseResponseDto, CreateWorkoutPlanExerciseDto, WorkoutPlanExerciseUpdateDto>
{
}

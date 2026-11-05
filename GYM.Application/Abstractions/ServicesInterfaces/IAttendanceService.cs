using GYM.Application.DTOs.Attendance;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IAttendanceService :
    IService<Attendance, AttendanceResponseDto, CreateAttendanceDto, AttendanceUpdateDto>
{
}

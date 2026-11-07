using GYM.Application.Abstractions;
using GYM.Application.DTOs.Attendance;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IAttendanceService :
    IService<Attendance, AttendanceResponseDto, CreateAttendanceDto, AttendanceUpdateDto>
{
}

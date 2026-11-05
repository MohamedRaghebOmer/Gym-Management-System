using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Attendance;

public sealed record AttendanceResponseDto(
    int Id,
    int MemberId,
    DateTime CheckIn,
    DateTime? CheckOut,
    DateTime CreatedAt)
    : IResponseDto<Domain.Entities.Attendance, AttendanceResponseDto>
{
    public static AttendanceResponseDto FromEntity(Domain.Entities.Attendance entity)
    {
        return new AttendanceResponseDto(
            Id: entity.Id.Value,
            MemberId: entity.MemberId.Value,
            CheckIn: entity.CheckIn,
            CheckOut: entity.CheckOut,
            CreatedAt: entity.CreatedAt);
    }
}

using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Attendance;

public sealed record CreateAttendanceDto : ICreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive integer.")]
    public required int MemberId { get; init; }

    [Required]
    [DataType(DataType.DateTime, ErrorMessage = "CheckIn must be a valid date time.")]
    public required DateTime CheckIn { get; init; }

    [DataType(DataType.DateTime, ErrorMessage = "CheckOut must be a valid date time.")]
    public DateTime? CheckOut { get; init; }
}

using GYM.Application.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace GYM.Application.DTOs.Attendance;

public sealed record AttendanceUpdateDto : IUpdateDto
{
    [Required]
    [DataType(DataType.DateTime, ErrorMessage = "CheckIn must be a valid date time.")]
    public required DateTime CheckIn { get; init; }

    [DataType(DataType.DateTime, ErrorMessage = "CheckOut must be a valid date time.")]
    public DateTime? CheckOut { get; init; }
}

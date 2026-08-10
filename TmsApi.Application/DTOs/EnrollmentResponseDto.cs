
namespace TmsApi.Application.DTOs;

public record EnrollmentResponseDto(
    int Id,
    string StudentName,
    string CourseName,
    string Status,
    DateTime EnrolledAt
);
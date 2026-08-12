namespace TmsApi.Application.Dtos;

public record EnrollmentResponseDto(
    int Id,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    int StudentId,
    string StudentName,
    string Status,
    DateTime EnrolledAt
);
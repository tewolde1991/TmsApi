namespace TmsApi.Application.Dtos;

public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string FirstName,
    string LastName,
    string Email,
    decimal GPA,
    bool IsActive,
    int EnrollmentCount,
    IReadOnlyList<StudentCourseDto>? Courses = null
);

public record StudentCourseDto(
    int CourseId,
    string CourseCode,
    string Title
);
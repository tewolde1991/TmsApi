

namespace TmsApi.Application.Grades.Dtos;

public record GradeResponseDto(
    int Id,
    int StudentId,
    int CourseId,
    decimal Score
);
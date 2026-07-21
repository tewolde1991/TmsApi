namespace TmsApi.Application.Dtos;

public record StudentDto(
int Id,
string FullName,
string Email
)
{
  // Using the primary constructor-generated properties: Id, FullName, Email
}

public record CourseEnrollmentDto(
string CourseTitle,
int EnrollmentCount
);
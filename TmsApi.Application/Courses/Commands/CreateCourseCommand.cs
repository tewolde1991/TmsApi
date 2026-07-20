
using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Courses.Commands;
public record CreateCourseCommands(string Code, string Title, int MaxCapacity): IRequest<Result<CourseCreateDto, CourseError>>;

public record CourseCreateDto(
    int Id, 
    string Code, 
    string Title,
    int MaxCapacity);
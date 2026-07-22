using MediatR;

namespace TmsApi.Application.Courses.Queries;

public record SearchCoursesQuery(string? Term)
    : IRequest<IReadOnlyList<CourseListItemDto>>;
using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class SearchCoursesHandler(
    ICourseRepository repo)
    : IRequestHandler<SearchCoursesQuery, IReadOnlyList<CourseListItemDto>>
{
    public async Task<IReadOnlyList<CourseListItemDto>> Handle(
        SearchCoursesQuery request,
        CancellationToken ct)
    {
        var courses = await repo.SearchAsync(
            request.Term,
            ct);


        return courses
            .Select(c => new CourseListItemDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToList();
    }
}
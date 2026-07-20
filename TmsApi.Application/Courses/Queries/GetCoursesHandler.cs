using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCoursesHandler(ICourseRepository courseRepo)
    : IRequestHandler<GetCoursesQuery, GetCoursesResult>
{
    public async Task<GetCoursesResult> Handle(
        GetCoursesQuery query,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var totalCount = await courseRepo.CountAsync(ct);

        var courses = await courseRepo.GetPagedWithEnrollmentsAsync(
            page,
            pageSize,
            ct);

        var rows = courses
            .Select(c => new CourseListItemDto(
                c.Id,
                c.Title,
                c.Code,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        return new GetCoursesResult(
            rows,
            totalCount,
            page,
            pageSize,
            totalPages,
            hasNext,
            hasPrevious);
    }
}
using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCoursesHandler(ICachedCourseService cachedService)
    : IRequestHandler<GetCoursesQuery, GetCoursesResult>
{
    public async Task<GetCoursesResult> Handle(
        GetCoursesQuery query,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var allCourses = await cachedService.GetAllCoursesAsync(ct);

        var totalCount = allCourses.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        var pageItems = allCourses
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var rows = pageItems
            .Select(c => new CourseListItemDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.EnrollmentCount))
            .ToList();

        return new GetCoursesResult(
            Data: rows,
            Meta: new PaginationMeta(
                TotalCount: totalCount,
                Page: page,
                PageSize: pageSize,
                TotalPages: totalPages,
                HasNext: hasNext,
                HasPrevious: hasPrevious
            ),
            Links: []
        );
    }
}
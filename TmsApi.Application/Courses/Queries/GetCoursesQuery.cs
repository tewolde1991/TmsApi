using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Queries;

public record GetCoursesQuery(
    int Page,
    int PageSize
) : IRequest<GetCoursesResult>;

public record CourseListItemDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount
);

public record PaginationMeta(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNext,
    bool HasPrevious
);

public record GetCoursesResult(
    IReadOnlyList<CourseListItemDto> Data,
    PaginationMeta Meta,
    IReadOnlyList<LinkDto> Links
);

using MediatR;

namespace TmsApi.Application.Courses.Queries;
public record GetCoursesQuery(
    int Page,
    int PageSize): IRequest<GetCoursesResult>;

public record  CourseListItemDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity, 
    int EnrollmentCount
);

public record GetCoursesResult(
    IReadOnlyList<CourseListItemDto> Data, 
    int TotalCount,
    int Page,
    int PageSize,
    int totalPages,
    bool hasNext,
    bool hasPrevious);
using MediatR;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Queries;

public record GetCourseByIdQuery(int Id) : IRequest<CourseResponseDto?>;
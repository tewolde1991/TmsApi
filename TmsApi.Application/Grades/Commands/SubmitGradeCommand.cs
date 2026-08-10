

using MediatR;
using TmsApi.Application.Grades.Dtos;

namespace TmsApi.Application.Grades.Commands;

public record SubmitGradeCommand(int StudentId, int CourseId, decimal Score) : IRequest<GradeResponseDto>;
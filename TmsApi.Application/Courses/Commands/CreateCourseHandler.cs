using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseHandler(ICourseRepository courseRepo):IRequestHandler<CreateCourseCommands, Result<CourseCreateDto, CourseError>>{


    public async Task<Result<CourseCreateDto, CourseError>> Handle(
        CreateCourseCommands command,
        CancellationToken ct
    )
    {
        // check if course code exists
        var existing = await courseRepo.GetByCodeAsync(command.Code, ct);

        if (existing is not null){
            return Result<CourseCreateDto, CourseError>.Failure(CourseError.DuplicateCode(command.Code));
        }

        var course = new Course{
            Code = command.Code,
            Title = command.Title,
            MaxCapacity = command.MaxCapacity
        };
        // await courseRepo.AddAsync(course, ct);

        var dto = new CourseCreateDto(
            course.Id,
            course.Code,
            course.Title,
            course.MaxCapacity
        );

        return Result<CourseCreateDto, CourseError>.Success(dto);
    }
}
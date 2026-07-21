using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseHandler(
    ICourseRepository repo,
    ICachedCourseService cachedService)
    : IRequestHandler<UpdateCourseCommand, bool>
{
    public async Task<bool> Handle(UpdateCourseCommand command,
        CancellationToken ct)
    {
        var course = await repo.GetByIdAsync(command.Id, ct);
        if (course is null)
        {
            return false;
        }

        course.Title = command.Title;
        course.Code = command.Code;
            course.MaxCapacity = command.MaxCapacity;
        await repo.UpdateAsync(course, ct);
        await cachedService.InvalidateCourseCacheAsync(ct);
        return true;
    }
}


using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Application.Authorization;
using TmsApi.Domain.Entities;

public class CourseInstructorHandler: AuthorizationHandler<CourseInstructorRequirement, Course>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseInstructorRequirement requirement, Course resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isInstructor = context.User.IsInRole("Instructor");
        var isAdmin = context.User.IsInRole("Admin");

        // admin can manage any resource
        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;

        }
        // instructor can only manage course where instructorId matches their user ID
        if (isInstructor && resource.InstructorId == userId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
    
}
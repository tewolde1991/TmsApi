using Microsoft.EntityFrameworkCore;
using Tms.Api.Dtos;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger) 
    : IEnrollmentService
{
    public async Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt 
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId, 
        EnrollStudentRequest request, 
        CancellationToken ct)
    {
        // Business Rule 1: Check if course exists
        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);

        if (course == null)
            throw new InvalidOperationException($"Course with ID {courseId} not found.");

        // Business Rule 2: Check course capacity
        var currentCount = await context.Enrollments
            .CountAsync(e => e.CourseId == courseId, ct);

        if (currentCount >= course.MaxCapacity)
            throw new InvalidOperationException($"Course '{course.Title}' is full (Capacity: {course.MaxCapacity}).");

        // Create new enrollment
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
            // Status = "Active",
            Year = DateTime.UtcNow.Year
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Student {StudentId} enrolled in course {CourseId}", 
            request.StudentId, courseId);

        // Return consistent DTO
        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

   
}
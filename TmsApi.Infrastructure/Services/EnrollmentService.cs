using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Services;

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
                e.Student.FirstName + " " + e.Student.LastName,
                e.Course.Title,
                e.Status.ToString(),
                e.EnrolledAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);

        if (course == null)
            throw new InvalidOperationException($"Course with ID {courseId} not found.");

        var currentCount = await context.Enrollments
            .CountAsync(e => e.CourseId == courseId, ct);

        if (currentCount >= course.MaxCapacity)
            throw new InvalidOperationException($"Course '{course.Title}' is full (Capacity: {course.MaxCapacity}).");

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
            Year = DateTime.UtcNow.Year
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Student {StudentId} enrolled in course {CourseId}",
            request.StudentId, courseId);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.Student.FirstName + " " + e.Student.LastName,
                e.Course.Title,
                e.Status.ToString(),
                e.EnrolledAt
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.Student.FirstName + " " + e.Student.LastName,
                e.Course.Title,
                e.Status.ToString(),
                e.EnrolledAt
            ))
            .ToListAsync(ct);
    }
}
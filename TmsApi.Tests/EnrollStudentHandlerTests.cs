using NSubstitute;
using TmsApi.Application.Common;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

public class EnrollStudentHandlerTests
{
    [Fact]
    public async Task Handle_WhenAlreadyEnrolled_ReturnsDuplicateError()
    {
        // ────────────────────── ARRANGE ──────────────────────
        var enrollmentRepo = Substitute.For<IEnrollmentRepository>();
        var courseRepo = Substitute.For<ICourseRepository>();

        // Student 99 is already enrolled in CS-401
        enrollmentRepo
            .ExistsAsync(99, "CS-401", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var course = new Course
        {
            Id = 1,
            Code = "CS-401",
            Title = "Advanced Web Dev",
            MaxCapacity = 30,
            Enrollments = new List<Enrollment>()
        };

        courseRepo
            .GetByCodeAsync("CS-401", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Course?>(course));

        // Use repositories — matching the real handler constructor
        var handler = new EnrollStudentHandler(enrollmentRepo, courseRepo);

        var command = new EnrollStudentCommand(StudentId: 99, CourseCode: "CS-401");

        // ────────────────────── ACT ──────────────────────
        var result = await handler.Handle(command, CancellationToken.None);

        // ────────────────────── ASSERT ──────────────────────
        Assert.False(result.IsSuccess);
        Assert.Equal("already_enrolled", result.Error.Code);
        Assert.Equal(EnrollmentError.AlreadyEnrolled(99, "CS-401"), result.Error);

        await enrollmentRepo
            .DidNotReceive()
            .AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }
}
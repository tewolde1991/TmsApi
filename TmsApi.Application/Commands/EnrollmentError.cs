namespace TmsApi.Application.Commands;

public record EnrollmentError(
    string Code,
    string Message)
{
    public static EnrollmentError CourseNotFound =>
        new(
            "course_not_found",
            "The specified course was not found.");

    public static EnrollmentError CourseFull =>
        new(
            "course_full",
            "The course has reached its maximum capacity.");

    public static EnrollmentError AlreadyEnrolled =>
        new(
            "already_enrolled",
            "The student is already enrolled in this course.");

    public static EnrollmentError StudentNotFound =>
        new(
            "student_not_found",
            "The specified student was not found.");
}
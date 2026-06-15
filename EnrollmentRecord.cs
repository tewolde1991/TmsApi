namespace TmsApi;

public record EnrollmentRecord(
    string Id,
    string studentId,
    string courseCode,
    DateTime EnrolledAt
);


namespace TmsApi.Application.DTOs;
public record CreateStudentRequest(
    string RegistrationNumber,
    string FirstName,
    string LastName);
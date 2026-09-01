using MediatR;
namespace TmsApi.Application.Students.Commands;

public record CreateStudentCommand(
    string FirstName,
    string LastName,
    string RegistrationNumber,
    bool IsActive)
: IRequest<StudentCreatedDto>;

public record StudentCreatedDto(
    int Id,
    string FirstName,
    string LastName,
    string RegistrationNumber,
    bool IsActive
);
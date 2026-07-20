
using MediatR;
namespace TmsApi.Application.Students.Commands;
public record CreateStudentCommand(
    string Name,
    string RegistrationNumber,
    bool IsActive)
: IRequest<StudentCreatedDto>;


public record StudentCreatedDto(
    int Id,
    string Name,
    string RegistrationNumber,
    bool IsActive
);
// public interface IStudentService
// {
//   Task<IEnumerable<Student>> GetAllAsync();
//   Task<Student?> GetByIdAsync(string id);
//   Task<Student> CreateAsync(string firstName, string lastName, string email);
//   Task<bool> DeleteAsync(string id);
// }

//  module 6
using TmsApi.Dtos;

namespace TmsApi.Services;

public interface IStudentService
{
  Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
  Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
  Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct);
  Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
  Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct);
  Task<bool> DeleteAsync(int id, CancellationToken ct);
}
using HostelStudentTracking.Models;

namespace HostelStudentTracking.Services
{
    public interface IStudentService
    {
        Task<List<Student>> GetAsync();
        Task<Student?> GetAsync(string id);
        Task<Student> CreateAsync(CreateStudentRequest student);
        Task UpdateAsync(string id, CreateStudentRequest studentIn);
        Task RemoveAsync(string id);
        Task<Student?> GetByRollNoAsync(string rollNo);
        Task<Student?> GetByEmailAsync(string email);
        Task<Student?> GetByNameAsync(string name);
        Task<IEnumerable<Student>> GetAllStudentsAsync();

    }
}
using HostelStudentTracking.Models;

namespace HostelStudentTracking.Services
{
    public interface IAdminService
    {
        Task<List<Admin>> GetAllAdminsAsync();
        Task<Admin?> GetAdminByIdAsync(string id);
        Task<Admin?> GetAdminByEmailAsync(string email);
        Task<Admin> CreateAdminAsync(Admin admin);
        Task UpdateAdminAsync(string id, Admin updatedAdmin);
        Task DeleteAdminAsync(string id);
    }
}

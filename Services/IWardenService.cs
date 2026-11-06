using HostelStudentTracking.Models;

namespace HostelStudentTracking.Services
{
    public interface IWardenService
    {
        Task<Warden> RegisterAsync(Warden warden);
        Task<List<Warden>> GetAllAsync();
        Task<Warden?> GetByIdAsync(string id);
        Task UpdateAsync(string id, Warden warden);
        Task DeleteAsync(string id);
    }
}

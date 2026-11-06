using HostelStudentTracking.Models;
using MongoDB.Driver;
using BCrypt.Net;

namespace HostelStudentTracking.Services
{
    public class AdminService : IAdminService
    {
        private readonly IMongoCollection<Admin> _admins;

        public AdminService(IMongoDatabase database)
        {
            _admins = database.GetCollection<Admin>("Admins");
        }

        public async Task<List<Admin>> GetAllAdminsAsync() =>
            await _admins.Find(_ => true).ToListAsync();

        public async Task<Admin?> GetAdminByIdAsync(string id) =>
            await _admins.Find(a => a.Id == id).FirstOrDefaultAsync();

        public async Task<Admin?> GetAdminByEmailAsync(string email) =>
            await _admins.Find(a => a.Email == email).FirstOrDefaultAsync();

        public async Task<Admin> CreateAdminAsync(Admin admin)
        {
            admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);
            await _admins.InsertOneAsync(admin);
            return admin;
        }

        public async Task UpdateAdminAsync(string id, Admin updatedAdmin) =>
            await _admins.ReplaceOneAsync(a => a.Id == id, updatedAdmin);

        public async Task DeleteAdminAsync(string id) =>
            await _admins.DeleteOneAsync(a => a.Id == id);
    }
}

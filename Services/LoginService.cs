using HostelStudentTracking.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using BCrypt.Net;

namespace HostelStudentTracking.Services
{
    public class LoginService : ILoginService
    {
        private readonly IMongoCollection<Admin> _admins;
        private readonly IMongoCollection<Warden> _wardens;
        private readonly IMongoCollection<Student> _students;

        public LoginService(IMongoDatabase database)
        {
            // Use exact collection names as they exist in MongoDB
            _admins = database.GetCollection<Admin>("Admins");
            _wardens = database.GetCollection<Warden>("Wardens");
            _students = database.GetCollection<Student>("students");
        }

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            var role = request.Role?.Trim().ToLower();

            switch (role)
            {
                case "admin":
                    return await AuthenticateAdminAsync(request.Email, request.Password);
                case "warden":
                    return await AuthenticateWardenAsync(request.Email, request.Password);
                case "student":
                    return await AuthenticateStudentAsync(request.Email, request.Password);
                default:
                    return new LoginResponse { Success = false, Message = "Invalid role." };
            }
        }

        private async Task<LoginResponse> AuthenticateAdminAsync(string email, string password)
        {
            var admin = await _admins
              .Find(s => s.Email.ToLower() == email.ToLower())
              .FirstOrDefaultAsync();
            if (admin == null)
                return new LoginResponse { Success = false, Message = "Invalid credentials" };
           
            if (!BCrypt.Net.BCrypt.Verify(password, admin.Password))
                return new LoginResponse { Success = false, Message = "Invalid credentials" };

            return new LoginResponse
            {
                Success = true,
                Message = "Admin login verified",
                Role = "admin",
                UserDetails = new { admin.Id, admin.Username, admin.Email }
            };
        }

        private async Task<LoginResponse> AuthenticateWardenAsync(string email, string password)
        {
            var warden = await _wardens
                .Find(w => w.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
    
            if (warden == null)
            return new LoginResponse { Success = false, Message = "Invalid credentials" };

        // Verify hashed password
            if (!BCrypt.Net.BCrypt.Verify(password, warden.Password))
                return new LoginResponse { Success = false, Message = "Invalid credentials" };

            return new LoginResponse
            {
                Success = true,
                Message = "Warden login verified",
                Role = "warden",
                UserDetails = new { warden.Id, warden.Name, warden.Email, warden.WardenCode }
            };
        }
        private async Task<LoginResponse> AuthenticateStudentAsync(string email, string password)
        {
           var student = await _students
               .Find(s => s.Email.ToLower() == email.ToLower() && s.Password == password)
               .FirstOrDefaultAsync();

            if (student == null)
                return new LoginResponse { Success = false, Message = "Invalid credentials" };

            return new LoginResponse
            {
                Success = true,
                Message = "Student login verified",
                Role = "student",
                UserDetails = new { student.Id, student.Name, student.Email, student.RollNo, student.RoomNo }
            };
        }
    }
}
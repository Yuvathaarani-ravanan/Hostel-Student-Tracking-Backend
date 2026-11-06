using HostelStudentTracking.Models;
using System.Threading.Tasks;

namespace HostelStudentTracking.Services
{
    public interface ILoginService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);
    }
}

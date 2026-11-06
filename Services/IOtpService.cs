using System.Threading.Tasks;

namespace HostelStudentTracking.Services
{
    public interface IOtpService
    {
        Task<bool> SendOtpAsync(string email, string role);
        Task<bool> VerifyOtpAsync(string email, string otp);
    }
}

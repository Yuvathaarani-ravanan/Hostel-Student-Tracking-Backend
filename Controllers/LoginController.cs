using HostelStudentTracking.Models;
using HostelStudentTracking.Services;
using Microsoft.AspNetCore.Mvc;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IOtpService _otpService;

        public LoginController(ILoginService loginService, IOtpService otpService)
        {
            _loginService = loginService;
            _otpService = otpService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new { message = "Email, Password, and Role are required." });
            }

            var response = await _loginService.AuthenticateAsync(request);

            if (!response.Success)
                return Unauthorized(new { message = response.Message });

            // ✅ Send OTP after successful login validation
            var otpSent = await _otpService.SendOtpAsync(request.Email, request.Role);
            if (!otpSent)
                return StatusCode(500, new { message = "Login successful, but failed to send OTP." });

            return Ok(new { message = "Login validated. OTP sent to your email for verification." });
        }
    }
}

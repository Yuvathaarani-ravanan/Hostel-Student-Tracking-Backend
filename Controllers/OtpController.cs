using HostelStudentTracking.Models;
using HostelStudentTracking.Services;
using Microsoft.AspNetCore.Mvc;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            var verified = await _otpService.VerifyOtpAsync(request.Email, request.Otp);

            if (!verified)
                return BadRequest(new { success = false, message = "Invalid or expired OTP." });

            return Ok(new { success = true, message = "OTP verified successfully! You are now logged in." });
        }
    }
}

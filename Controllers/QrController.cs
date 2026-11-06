using Microsoft.AspNetCore.Mvc;
using HostelStudentTracking.Services;
using System.Threading.Tasks;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrController : ControllerBase
    {
        private readonly QrService _qrService;

        public QrController(QrService qrService)
        {
            _qrService = qrService;
        }

        // ✅ Generate QR
        [HttpGet("generate")]
        public async Task<IActionResult> GenerateQr()
        {
            var result = await _qrService.GenerateQrAsync();

            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var istExpiry = TimeZoneInfo.ConvertTimeFromUtc(result.ExpiresAt, istZone);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                imageUrl = result.ImageUrl,
                token = result.Token,
                session = result.Session,
                expiresAt = istExpiry.ToString("yyyy-MM-dd hh:mm tt") + " IST"
            });
        }

     [HttpGet("check-active")]
public async Task<IActionResult> CheckActiveQr()
{
    var result = await _qrService.CheckActiveQrAsync();
    return Ok(result);
}

        // ✅ Scan QR using roll number
        [HttpGet("scan")]
        public async Task<IActionResult> ScanQr([FromQuery] string token, [FromQuery] string rollNo)
        {
            var result = await _qrService.ScanQrAsync(token, rollNo);

            return Ok(new
            {
                success = result.Success,
                message = result.Message
            });
        }
    }
}

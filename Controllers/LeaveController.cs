using Microsoft.AspNetCore.Mvc;
using HostelStudentTracking.Models;
using HostelStudentTracking.Services;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly LeaveService _leaveService;

        public LeaveController(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        // Create leave request
        [HttpPost("create")]
        public async Task<IActionResult> CreateLeave([FromBody] LeaveRequest leave)
        {
            if (leave.FromDate > leave.ToDate)
                return BadRequest("Invalid date");

            await _leaveService.CreateAsync(leave);
            return Ok("Leave request submitted");
        }

        // Get all leave requests
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _leaveService.GetAllAsync();
            return Ok(data);
        }

        // Approve / Reject
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(string id, [FromQuery] string status)
        {
            await _leaveService.UpdateStatusAsync(id, status);
            return Ok("Status updated");
        }
    }
}

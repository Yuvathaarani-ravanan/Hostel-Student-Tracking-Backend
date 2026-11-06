using Microsoft.AspNetCore.Mvc;
using HostelStudentTracking.Models;
using HostelStudentTracking.Services;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WardenController : ControllerBase
    {
        private readonly IWardenService _wardenService;

        public WardenController(IWardenService wardenService)
        {
            _wardenService = wardenService;
        }

        // ✅ Create/Register new warden
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Warden warden)
        {
            var created = await _wardenService.RegisterAsync(warden);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ Get all wardens
        [HttpGet("get")]
        public async Task<ActionResult<List<Warden>>> GetAll()
        {
            var wardens = await _wardenService.GetAllAsync();
            return Ok(wardens);
        }

        // ✅ Get warden by ID
        [HttpGet("get/{id}")]
        public async Task<ActionResult<Warden>> GetById(string id)
        {
            var warden = await _wardenService.GetByIdAsync(id);
            if (warden == null) return NotFound(new { message = "Warden not found" });
            return Ok(warden);
        }

        // ✅ Update warden
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Warden warden)
        {
            try
            {
                await _wardenService.UpdateAsync(id, warden);
                return Ok(new { message = "Warden updated successfully" });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Invalid Warden ID" });
            }
        }

        // ✅ Delete warden
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _wardenService.DeleteAsync(id);
                return Ok(new { message = "Warden deleted successfully" });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Invalid Warden ID" });
            }
        }
    }
}

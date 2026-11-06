using HostelStudentTracking.Models;
using HostelStudentTracking.Services;
using Microsoft.AspNetCore.Mvc;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ✅ GET all admins
        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(admins);
        }

        // ✅ GET admin by ID
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Admin>> GetAdminById(string id)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null)
                return NotFound(new { message = "Admin not found" });

            return Ok(admin);
        }

        // ✅ POST create new admin
        [HttpPost("create")]
        public async Task<ActionResult<Admin>> CreateAdmin([FromBody] Admin admin)
        {
            var existing = await _adminService.GetAdminByEmailAsync(admin.Email);
            if (existing != null)
                return BadRequest(new { message = "Email already exists" });

            await _adminService.CreateAdminAsync(admin);
            return CreatedAtAction(nameof(GetAdminById), new { id = admin.Id }, admin);
        }

        // ✅ PUT update admin
        [HttpPut("update/{id:length(24)}")]
        public async Task<IActionResult> UpdateAdmin(string id, [FromBody] Admin updatedAdmin)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null)
                return NotFound(new { message = "Admin not found" });

            updatedAdmin.Id = id;
            await _adminService.UpdateAdminAsync(id, updatedAdmin);
            return Ok(new { message = "Admin updated successfully" });
        }

        // ✅ DELETE admin
        [HttpDelete("delete/{id:length(24)}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null)
                return NotFound(new { message = "Admin not found" });

            await _adminService.DeleteAdminAsync(id);
            return Ok(new { message = "Admin deleted successfully" });
        }
    }
}

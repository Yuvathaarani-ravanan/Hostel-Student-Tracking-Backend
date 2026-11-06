using Microsoft.AspNetCore.Mvc;
using HostelStudentTracking.Models;
using HostelStudentTracking.Services;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly AttendanceService _attendanceService;

        public StudentsController(IStudentService studentService, AttendanceService attendanceService)
        {
            _studentService = studentService;
            _attendanceService = attendanceService;
        }

        [HttpGet("get")]
        public async Task<ActionResult<List<Student>>> Get()
        {
            var students = await _studentService.GetAsync();
            return Ok(new { 
                success = true, 
                data = students,
                count = students.Count 
            });
        }

        [HttpGet("get/{id:length(24)}")]
        public async Task<ActionResult<Student>> Get(string id)
        {
            var student = await _studentService.GetAsync(id);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            return Ok(new { success = true, data = student });
        }

        [HttpGet("get/rollno/{rollNo}")]
        public async Task<ActionResult<Student>> GetByRollNo(string rollNo)
        {
            var student = await _studentService.GetByRollNoAsync(rollNo);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            return Ok(new { success = true, data = student });
        }

        [HttpGet("get/email/{email}")]
        public async Task<ActionResult<Student>> GetByEmail(string email)
        {
            var student = await _studentService.GetByEmailAsync(email);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            return Ok(new { success = true, data = student });
        }

        [HttpGet("get/name/{name}")]
        public async Task<ActionResult<Student>> GetByName(string name)
        {
            var student = await _studentService.GetByNameAsync(name);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            return Ok(new { success = true, data = student });
        }

        [HttpPost("create")]
        public async Task<ActionResult<Student>> Create(CreateStudentRequest student)
        {
            try
            {
                var createdStudent = await _studentService.CreateAsync(student);
                return CreatedAtAction(nameof(Get), new { id = createdStudent.Id }, new { 
                    success = true, 
                    message = "Student created successfully",
                    data = createdStudent 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating the student." });
            }
        }

        [HttpPut("update/{id:length(24)}")]
        public async Task<IActionResult> Update(string id, CreateStudentRequest studentIn)
        {
            var student = await _studentService.GetAsync(id);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            await _studentService.UpdateAsync(id, studentIn);

            return Ok(new { success = true, message = "Student updated successfully" });
        }

        [HttpDelete("delete/{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _studentService.GetAsync(id);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            await _studentService.RemoveAsync(id);

            return Ok(new { success = true, message = "Student deleted successfully" });
        }

       [HttpGet("all")]
public async Task<ActionResult> GetAllStudents()
{
    var students = await _studentService.GetAllStudentsAsync();

    return Ok(new
    {
        success = true,
        message = "All student details retrieved successfully",
        data = students
    });
}

[HttpGet("summary")]
public async Task<IActionResult> GetStudentSummary()
{
    var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
    var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

    string today = istNow.ToString("yyyy-MM-dd");
    string session = istNow.Hour < 12 ? "morning" : "evening";

    // 🔹 Get all students
    var students = await _studentService.GetAllStudentsAsync();

    // 🔹 Get all attendance from service
    var allAttendance = await _attendanceService.GetAllAttendanceAsync();

    // 🔹 Filter today's attendance for this session
    var attendanceToday = allAttendance
        .Where(a => a.Date == today && a.Session == session)
        .ToList();

    var summary = students.Select(s => new
    {
        s.Name,
        s.RollNo,
        s.Department,
        s.Year,
        s.RoomNo,
        s.Email,
        s.Mobile,
        s.Address,

        Attendance = attendanceToday.Any(a => a.RollNo == s.RollNo)
            ? "Present"
            : "Absent"
    }).ToList();

    return Ok(new
    {
        success = true,
        date = today,
        session = session,
        data = summary
    });
}

    }
}


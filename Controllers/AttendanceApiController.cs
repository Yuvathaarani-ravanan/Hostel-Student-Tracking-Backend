using Microsoft.AspNetCore.Mvc;
using HostelStudentTracking.Services;
using Microsoft.AspNetCore.SignalR;
using HostelStudentTracking.Hubs;
using System;
using System.Threading.Tasks;

namespace HostelStudentTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceApiController : ControllerBase
    {
        private readonly AttendanceService _attendanceService;
        private readonly IHubContext<AttendanceHub> _hubContext;

        public AttendanceApiController(AttendanceService attendanceService, IHubContext<AttendanceHub> hubContext)
        {
            _attendanceService = attendanceService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// ✅ Common attendance marking endpoint (for both Manual and QR)
        /// </summary>
        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance(
            [FromQuery] string rollNo,
            [FromQuery] string markedBy = "System",
            [FromQuery] string session = "morning",
            [FromQuery] string markType = "Manual")
        {
            if (string.IsNullOrEmpty(rollNo) || string.IsNullOrEmpty(session))
                return BadRequest(new { 
                    success = false, 
                    message = "RollNo and Session are required." 
                });

            try
            {
                var (success, message, attendance) = await _attendanceService.MarkAttendanceAsync(
                    rollNo, markedBy, session, markType);

                if (!success)
                    return BadRequest(new { success = false, message });

                var stats = await _attendanceService.GetLiveAttendanceStatsAsync(session);

                return Ok(new
                {
                    success = true,
                    message,
                    data = attendance,
                    stats = new { stats.Present, stats.Absent, stats.Total }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error marking attendance", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// ✅ Mark manual attendance (warden use)
        /// </summary>
        [HttpPost("manual")]
        public async Task<IActionResult> MarkManualAttendance(
            [FromQuery] string rollNo,
            [FromQuery] string markedBy,
            [FromQuery] string session)
        {
            return await MarkAttendance(rollNo, markedBy, session, "Manual");
        }

        /// <summary>
        /// ✅ Mark QR attendance (student scanning)
        /// </summary>
        [HttpPost("qr")]
        public async Task<IActionResult> MarkQRAttendance(
            [FromQuery] string rollNo,
            [FromQuery] string session)
        {
            return await MarkAttendance(rollNo, "QR Scanner", session, "QR");
        }

        /// <summary>
        /// ✅ Get student summary with Present/Absent status
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetStudentSummary([FromQuery] string session)
        {
            if (string.IsNullOrEmpty(session))
                return BadRequest(new { 
                    success = false, 
                    message = "Session is required (morning/evening)" 
                });

            try
            {
                var summary = await _attendanceService.GetStudentSummaryAsync(session);
                var stats = await _attendanceService.GetLiveAttendanceStatsAsync(session);

                return Ok(new
                {
                    success = true,
                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                    session = session,
                    stats = new {
                        stats.Present,
                        stats.Absent,
                        stats.Total
                    },
                    data = summary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error getting summary", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// ✅ Get live attendance statistics only
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetLiveStats([FromQuery] string session)
        {
            if (string.IsNullOrEmpty(session))
                return BadRequest(new { 
                    success = false, 
                    message = "Session is required (morning/evening)" 
                });

            try
            {
                var stats = await _attendanceService.GetLiveAttendanceStatsAsync(session);

                return Ok(new
                {
                    success = true,
                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                    session = session,
                    stats = new {
                        stats.Present,
                        stats.Absent,
                        stats.Total
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error getting stats", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// ✅ WebSocket test endpoint
        /// </summary>
        
        [HttpGet("test-websocket-real")]
public async Task<IActionResult> TestWebSocketReal()
{
    try
    {
        var results = new List<object>();
        
        // Test both sessions
        var sessions = new[] { "morning", "evening" };
        
        foreach (var session in sessions)
        {
            var stats = await _attendanceService.GetLiveAttendanceStatsAsync(session);
            
            // Send ACTUAL stats via SignalR (no artificial changes)
            await _hubContext.Clients.All.SendAsync("ReceiveStatsUpdate", new
            {
                Session = session,
                Total = stats.Total,
                Present = stats.Present,    // ACTUAL value
                Absent = stats.Absent,      // ACTUAL value
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Test = true,
                Message = $"Current stats for {session} session"
            });
            
            results.Add(new
            {
                Session = session,
                Stats = new { stats.Total, stats.Present, stats.Absent },
                Message = $"Sent current stats for {session} session"
            });
            
            await Task.Delay(300);
        }
        
        return Ok(new 
        { 
            success = true, 
            message = "Sent current stats via WebSocket",
            results = results
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}
/// <summary>
/// ✅ Simple WebSocket test
/// </summary>
[HttpGet("test-signalr")]
public async Task<IActionResult> TestSignalR([FromQuery] string session = "morning")
{
    try
    {
        var stats = await _attendanceService.GetLiveAttendanceStatsAsync(session);
        
        await _hubContext.Clients.All.SendAsync("ReceiveStatsUpdate", new
        {
            Session = session,
            Total = stats.Total,
            Present = stats.Present,
            Absent = stats.Absent,
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Message = "Test broadcast from API"
        });
        
        return Ok(new 
        { 
            success = true, 
            message = "SignalR test message sent",
            stats = new { stats.Total, stats.Present, stats.Absent }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

/// <summary>
/// ✅ Test attendance marking with WebSocket
/// </summary>
[HttpPost("test-mark")]
public async Task<IActionResult> TestMarkAttendance([FromQuery] string rollNo = "21BCE001", [FromQuery] string session = "morning")
{
    try
    {
        // First check if student exists
        var student = await _attendanceService.GetStudentByRollNoAsync(rollNo);
        
        if (student == null)
        {
            return BadRequest(new { 
                success = false, 
                message = $"Student {rollNo} not found. Using test data." 
            });
        }
        
        // Mark attendance for real student
        var (success, message, attendance) = await _attendanceService.MarkAttendanceAsync(
            rollNo, "Test System", session, "Test");
        
        if (!success && message.Contains("already marked"))
        {
            // If already marked, just get stats and broadcast
            var stats = await _attendanceService.GetLiveAttendanceStatsAsync(session);
            
            await _hubContext.Clients.All.SendAsync("ReceiveStatsUpdate", new
            {
                Session = session,
                Total = stats.Total,
                Present = stats.Present,
                Absent = stats.Absent,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Message = "Updated stats (already marked)"
            });
            
            return Ok(new 
            { 
                success = true, 
                message = "Attendance already marked. Sent updated stats.",
                stats = new { stats.Total, stats.Present, stats.Absent }
            });
        }
        
        return Ok(new 
        { 
            success = success, 
            message = message,
            data = attendance
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}
    }
}
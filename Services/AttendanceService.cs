using MongoDB.Driver;
using HostelStudentTracking.Models;
using Microsoft.AspNetCore.SignalR;
using HostelStudentTracking.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HostelStudentTracking.Services
{
    public class AttendanceService
    {
        private readonly IMongoCollection<Attendance> _attendanceCollection;
        private readonly IMongoCollection<Student> _studentCollection;
        private readonly IHubContext<AttendanceHub> _hubContext;

        public AttendanceService(IMongoDatabase database, IHubContext<AttendanceHub> hubContext)
        {
            _attendanceCollection = database.GetCollection<Attendance>("Attendance");
            _studentCollection = database.GetCollection<Student>("students");
            _hubContext = hubContext;
        }

        // ===================== MARK ATTENDANCE =====================
        public async Task<(bool Success, string Message, Attendance? Attendance)> MarkAttendanceAsync(
            string rollNo, string markedBy, string session, string markType = "Manual")
        {
            try
            {
                var student = await _studentCollection
                    .Find(s => s.RollNo.ToLower() == rollNo.ToLower())
                    .FirstOrDefaultAsync();

                if (student == null)
                    return (false, $"Student with RollNo {rollNo} not found", null);

                string today = DateTime.Now.ToString("yyyy-MM-dd");

                var existing = await _attendanceCollection
                    .Find(a =>
                        a.RollNo.ToLower() == rollNo.ToLower() &&
                        a.Date == today &&
                        a.Session.ToLower() == session.ToLower())
                    .FirstOrDefaultAsync();

                if (existing != null)
                    return (false, $"Attendance already marked for {rollNo} in {session} session", existing);

                var attendance = new Attendance
                {
                    Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                    Name = student.Name,
                    RollNo = student.RollNo,
                    Date = today,
                    Session = session,
                    Status = "Present",
                    MarkType = markType,
                    MarkedBy = markedBy,
                    MarkedAt = DateTime.Now
                };

                await _attendanceCollection.InsertOneAsync(attendance);

                // 🔥 LIVE SIGNALR UPDATE
                await BroadcastLiveStats(session);

                return (true, $"Attendance marked successfully for {student.Name}", attendance);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // ===================== SIGNALR BROADCAST =====================
        private async Task BroadcastLiveStats(string session)
        {
            try
            {
                var stats = await GetLiveAttendanceStatsAsync(session);

                await _hubContext.Clients.All.SendAsync("ReceiveStatsUpdate", new
                {
                    Session = session,
                    Present = stats.Present,
                    Absent = stats.Absent,
                    Total = stats.Total,
                    Timestamp = DateTime.Now.ToString("HH:mm:ss")
                });
                Console.WriteLine($"📡 SignalR Broadcast: {session} session - " +
                         $"Present: {stats.Present}, Absent: {stats.Absent}, Total: {stats.Total}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SignalR Broadcast Error: {ex.Message}");   
            }         
        }

        // ===================== LIVE STATS =====================
        public async Task<(int Present, int Absent, int Total)> GetLiveAttendanceStatsAsync(string session)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            var students = await GetAllStudentsAsync();
            var presentList = await GetAttendanceByDateSessionAsync(today, session);

            int present = presentList.Count;
            int total = students.Count;
            int absent = total - present;

            return (present, absent, total);
        }
        

        // ===================== STUDENT SUMMARY =====================
        public async Task<List<StudentSummary>> GetStudentSummaryAsync(string session)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            var students = await GetAllStudentsAsync();
            var attendance = await GetAttendanceByDateSessionAsync(today, session);

            return students.Select(s => new StudentSummary
            {
                Name = s.Name,
                RollNo = s.RollNo,
                Department = s.Department,
                Year = s.Year.ToString(),
                RoomNo = s.RoomNo,
                Email = s.Email,
                Mobile = s.Mobile,
                Address = s.Address,
                Attendance = attendance.Any(a => a.RollNo == s.RollNo) ? "Present" : "Absent"
            }).ToList();
        }

        // ===================== API SUPPORT METHODS =====================
        public async Task<bool> MarkManualAttendanceAsync(string rollNo, string markedBy, string session)
        {
            var result = await MarkAttendanceAsync(rollNo, markedBy, session, "Manual");
            return result.Success;
        }

        public async Task<bool> MarkQRAttendanceAsync(string rollNo, string session)
        {
            var result = await MarkAttendanceAsync(rollNo, "QR Scanner", session, "QR");
            return result.Success;
        }

        // ===================== REQUIRED BY CONTROLLERS =====================
        public async Task<List<Attendance>> GetAllAttendanceAsync()
        {
            return await _attendanceCollection
                .Find(_ => true)
                .SortByDescending(a => a.MarkedAt)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByRollNoAsync(string rollNo)
        {
            return await _studentCollection
                .Find(s => s.RollNo.ToLower() == rollNo.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _studentCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Attendance>> GetAttendanceByDateSessionAsync(string date, string session)
        {
            return await _attendanceCollection.Find(a =>
                a.Date == date &&
                a.Session.ToLower() == session.ToLower() &&
                a.Status.ToLower() == "present"
            ).ToListAsync();
        }
    }

    // ===================== DTO =====================
    public class StudentSummary
    {
        public string? Name { get; set; }
        public string? RollNo { get; set; }
        public string? Department { get; set; }
        public string? Year { get; set; }
        public string? RoomNo { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? Attendance { get; set; }
    }
}

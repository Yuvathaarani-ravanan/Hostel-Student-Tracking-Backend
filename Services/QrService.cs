using HostelStudentTracking.Models;
using HostelStudentTracking.Hubs;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using QRCoder;
using System;
using System.Threading.Tasks;

namespace HostelStudentTracking.Services
{
    public class QrService
    {
        private readonly IMongoCollection<QrCodeSession> _qrSessions;
        private readonly IMongoCollection<Attendance> _attendance;
        private readonly IHubContext<AttendanceHub> _hubContext;
        private readonly AttendanceService _attendanceService;

        public QrService(IMongoDatabase database, IHubContext<AttendanceHub> hubContext, AttendanceService attendanceService)
        {
            _qrSessions = database.GetCollection<QrCodeSession>("QrCodeSession");
            _attendance = database.GetCollection<Attendance>("Attendance");
            _hubContext = hubContext;
            _attendanceService = attendanceService;
        }

        // Get current session based on IST time
        private (string session, string sessionRange) GetCurrentSession(DateTime istNow)
        {
            // Morning: 12:00 AM to 11:59 AM
            // Evening: 12:00 PM to 11:59 PM
            if (istNow.Hour < 12)
            {
                return ("morning", "12:00 AM - 11:59 AM");
            }
            else
            {
                return ("evening", "12:00 PM - 11:59 PM");
            }
        }

        // Check if QR exists for today's session (not just active)
        private async Task<QrCodeSession?> GetExistingQrForTodaySession(string session, DateTime istNow)
        {
            var todayStart = istNow.Date; // 12:00 AM today
            var todayEnd = todayStart.AddDays(1); // 12:00 AM tomorrow

            return await _qrSessions.Find(x =>
                x.Session == session &&
                x.CreatedAt >= todayStart &&
                x.CreatedAt < todayEnd
            ).FirstOrDefaultAsync();
        }

        // =======================
        // ✅ GENERATE QR - ONCE PER SESSION PER DAY
        // =======================
        public async Task<(bool Success, string Message, string ImageUrl, string Token, string Session, DateTime ExpiresAt, string SessionRange)>
        GenerateQrAsync()
        {
            var utcNow = DateTime.UtcNow;
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var istNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
            var (session, sessionRange) = GetCurrentSession(istNow);

            // ✅ CHECK 1: Is there already a QR for this session today?
            var existingQr = await GetExistingQrForTodaySession(session, istNow);

            if (existingQr != null)
            {
                // ✅ CHECK 2: Is it still active (not expired)?
                if (existingQr.ExpiresAt > DateTime.UtcNow)
                {
                    // QR exists and is active - CANNOT generate new one
                    return (
                        false,
                        $"❌ QR already generated for {session} session. Valid until {existingQr.ExpiresAt:hh:mm tt}. Cannot generate new QR.",
                        existingQr.QrImage ?? "",
                        existingQr.Token,
                        session,
                        existingQr.ExpiresAt,
                        sessionRange
                    );
                }
                else
                {
                    // QR exists but expired - CANNOT generate new one until next session
                    return (
                        false,
                        $"❌ QR for {session} session has expired. Cannot generate new QR for this session. Wait for next session.",
                        "",
                        "",
                        session,
                        existingQr.ExpiresAt,
                        sessionRange
                    );
                }
            }

            // ✅ No QR exists for this session today - Generate new one
            string token = Guid.NewGuid().ToString();
            DateTime expiry = utcNow.AddHours(1); // Valid for 1 hour
            var istExpiry = TimeZoneInfo.ConvertTimeFromUtc(expiry, istZone);

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(
                $"http://localhost:5071/api/Qr/scan?token={token}",
                QRCodeGenerator.ECCLevel.Q
            );

            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(20);
            string imageUrl = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";

            var qrSession = new QrCodeSession
            {
                Token = token,
                Session = session,
                CreatedAt = utcNow,
                ExpiresAt = expiry,
                QrImage = imageUrl
            };

            await _qrSessions.InsertOneAsync(qrSession);

            return (
                true,
                $"✅ QR generated successfully for {session} session (valid 1 hour until {istExpiry:hh:mm tt}).",
                imageUrl,
                token,
                session,
                expiry,
                sessionRange
            );

        }

        // =======================
        // ✅ CHECK ACTIVE QR
        // =======================
        public async Task<object> CheckActiveQrAsync()
{
    var utcNow = DateTime.UtcNow;
    var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
    var istNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);

    var (session, sessionRange) = GetCurrentSession(istNow);

    var qr = await GetExistingQrForTodaySession(session, istNow);

    if (qr == null)
    {
        return new
        {
            status = "none",
            session = session,
            message = $"No QR generated for {session} session yet.",
            sessionRange = sessionRange
        };
    }

    var istExpiry = TimeZoneInfo.ConvertTimeFromUtc(qr.ExpiresAt, istZone);

    if (qr.ExpiresAt <= utcNow)
    {
        return new
        {
            status = "expired",
            session = qr.Session,
            qrImage = "",
            expiresAt = istExpiry.ToString("yyyy-MM-dd hh:mm:ss tt"),
            message = $"QR expired at {istExpiry:hh:mm tt}",
            sessionRange = sessionRange
        };
    }

    return new
    {
        status = "exists_active",
        session = qr.Session,
        qrImage = qr.QrImage,
        expiresAt = qr.ExpiresAt.ToString("o"), // ISO format for JS timer
        message = $"QR active until {istExpiry:hh:mm tt}",
        sessionRange = sessionRange,
        timeRemaining = (qr.ExpiresAt - utcNow).TotalSeconds
    };
}

        // =======================
        // ✅ SCAN QR
        // =======================
        public async Task<(bool Success, string Message)> ScanQrAsync(string token, string rollNo)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            var qr = await _qrSessions.Find(x => x.Token == token).FirstOrDefaultAsync();

            if (qr == null)
                return (false, "❌ Invalid QR code.");

            // Check if expired
            if (DateTime.UtcNow > qr.ExpiresAt)
                return (false, $"❌ QR expired at {qr.ExpiresAt:hh:mm tt}. Please wait for next session.");

            // Check if scanning in correct session
            var (currentSession, _) = GetCurrentSession(istNow);
            if (currentSession != qr.Session)
            {
                return (false, $"❌ This QR is for {qr.Session} session. Current session is {currentSession}.");
            }

            // ✅ Mark attendance using common service
            var (success, message, attendance) = await _attendanceService.MarkAttendanceAsync(
                rollNo, "QR Scanner", qr.Session, "QR");

            if (!success)
                return (false, message);

            return (true, $"✅ Attendance marked successfully for {qr.Session} session.");
        }
    }
}
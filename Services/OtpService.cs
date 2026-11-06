using MailKit.Net.Smtp;
using MimeKit;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HostelStudentTracking.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMongoDatabase _database;
        private static readonly ConcurrentDictionary<string, (string otp, DateTime expiry)> _otpStore = new();

        public OtpService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<bool> SendOtpAsync(string email, string role)
        {
            try
            {
                string otp = new Random().Next(100000, 999999).ToString();
                _otpStore[email] = (otp, DateTime.Now.AddMinutes(5));

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Hostel Tracker", "yuvaravanan@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "OTP Verification - Hostel Tracking System";

                message.Body = new TextPart("plain")
                {
                    Text = $"Hello {role},\n\nYour OTP is: {otp}\nIt expires in 5 minutes.\n\nRegards,\nHostel Tracking System"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("yuvaravanan@gmail.com", "ispc fjxl yael sesl"); // Replace with valid credentials
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OTP Send Error: {ex.Message}");
                return false;
            }
        }

        public Task<bool> VerifyOtpAsync(string email, string otp)
        {
            if (_otpStore.TryGetValue(email, out var stored))
            {
                if (stored.otp == otp && DateTime.Now <= stored.expiry)
                {
                    _otpStore.TryRemove(email, out _);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
    }
}

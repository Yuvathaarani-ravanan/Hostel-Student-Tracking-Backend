using HostelStudentTracking.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HostelStudentTracking.Services
{
    public class QrCleanupService : BackgroundService
    {
        private readonly IMongoCollection<QrCodeSession> _qrSessions;
        private readonly ILogger<QrCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Every 5 minutes

        public QrCleanupService(IMongoDatabase database, ILogger<QrCleanupService> logger)
        {
            _qrSessions = database.GetCollection<QrCodeSession>("QrCodeSession");
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧹 QrCleanupService started at: {time}", DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    var now = DateTime.UtcNow;
                    var deleteResult = await _qrSessions.DeleteManyAsync(
                        x => x.ExpiresAt < now,
                        cancellationToken: stoppingToken
                    );

                    _logger.LogInformation("✅ Deleted {count} expired QR sessions at {time}", 
                        deleteResult.DeletedCount, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error while cleaning up expired QR sessions.");
                }

                // Wait 1 hour before the next cleanup
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("🛑 QrCleanupService stopped at: {time}", DateTime.UtcNow);
        }
    }
}

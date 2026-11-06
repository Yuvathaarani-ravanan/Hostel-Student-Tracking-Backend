using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace HostelStudentTracking.Hubs
{
    public class AttendanceHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"✅ WebSocket Connected: {Context.ConnectionId}");
            await Clients.Caller.SendAsync("Connected", new
            {
                ConnectionId = Context.ConnectionId,
                Message = "Welcome to Live Attendance Hub",
                Timestamp = DateTime.Now
            });
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"⚠️ WebSocket Disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Simple test method
        public async Task SendTest()
        {
            await Clients.Caller.SendAsync("ReceiveTest", new
            {
                Message = "Hub is working!",
                Time = DateTime.Now
            });
        }
    }
}
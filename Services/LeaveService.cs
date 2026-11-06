using MongoDB.Driver;
using HostelStudentTracking.Models;

namespace HostelStudentTracking.Services
{
    public class LeaveService
    {
        private readonly IMongoCollection<LeaveRequest> _leaveCollection;

        public LeaveService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);

            _leaveCollection = database.GetCollection<LeaveRequest>("LeaveRequests");
        }

        public async Task<List<LeaveRequest>> GetAllAsync()
        {
            return await _leaveCollection.Find(_ => true).ToListAsync();
        }

        public async Task CreateAsync(LeaveRequest leave)
        {
            await _leaveCollection.InsertOneAsync(leave);
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            var filter = Builders<LeaveRequest>.Filter.Eq(x => x.RollNo, id);
            var update = Builders<LeaveRequest>.Update.Set(x => x.Status, status);

            await _leaveCollection.UpdateOneAsync(filter, update);
        }
    }
}

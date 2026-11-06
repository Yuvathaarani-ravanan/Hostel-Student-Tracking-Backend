using HostelStudentTracking.Models;
using MongoDB.Driver;
using BCrypt.Net;

namespace HostelStudentTracking.Services
{
    public class WardenService : IWardenService
    {
        private readonly IMongoCollection<Warden> _wardenCollection;

        public WardenService(IMongoDatabase database)
        {
            _wardenCollection = database.GetCollection<Warden>("Wardens");
        }

        // ✅ Create/Register new warden
        public async Task<Warden> RegisterAsync(Warden warden)
        {
            warden.Password = BCrypt.Net.BCrypt.HashPassword(warden.Password);
            await _wardenCollection.InsertOneAsync(warden);
            return warden;
        }

        // ✅ Get all wardens
        public async Task<List<Warden>> GetAllAsync()
        {
            return await _wardenCollection.Find(_ => true).ToListAsync();
        }

        // ✅ Get warden by ID
        public async Task<Warden?> GetByIdAsync(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _)) return null;
            return await _wardenCollection.Find(w => w.Id == id).FirstOrDefaultAsync();
        }

        // ✅ Update warden
        public async Task UpdateAsync(string id, Warden warden)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
                throw new ArgumentException("Invalid ObjectId format.");

            await _wardenCollection.ReplaceOneAsync(w => w.Id == id, warden);
        }

        // ✅ Delete warden
        public async Task DeleteAsync(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
                throw new ArgumentException("Invalid ObjectId format.");

            await _wardenCollection.DeleteOneAsync(w => w.Id == id);
        }
    }
}

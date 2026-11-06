using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HostelStudentTracking.Models
{
    [BsonIgnoreExtraElements] // Add this to be safe
    public class Warden
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("wardenCode")]
        public string WardenCode { get; set; } = string.Empty;

        [BsonElement("mobile")]
        public string Mobile { get; set; } = string.Empty;

        [BsonElement("qualification")]
        public string Qualification { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("dateOfJoining")]
        public DateTime DateOfJoining { get; set; }

        [BsonElement("experience")]
        public int Experience { get; set; }

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "warden"; // Changed to lowercase to match other models
    }
}
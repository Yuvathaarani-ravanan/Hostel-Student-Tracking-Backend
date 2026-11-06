using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HostelStudentTracking.Models
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("rollNo")]
        public string RollNo { get; set; } = string.Empty;

        [BsonElement("date")]
        public string Date { get; set; } = string.Empty;  // e.g. "2025-11-04"

        [BsonElement("session")]
        public string Session { get; set; } = string.Empty;  // "morning" / "evening"

        [BsonElement("status")]
        public string Status { get; set; } = "present";  // default value

        [BsonElement("markedAt")]
        public DateTime MarkedAt { get; set; } = DateTime.Now;

        [BsonElement("markType")]
        public string MarkType { get; set; } = "QR";  // "QR" or "Manual"

        [BsonElement("markedBy")]
        public string MarkedBy { get; set; } = string.Empty;  // e.g. "Warden"
    }
}

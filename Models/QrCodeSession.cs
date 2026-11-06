using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HostelStudentTracking.Models
{
    public class QrCodeSession
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("token")]
        public string Token { get; set; } = string.Empty;
        
        [BsonElement("qrImage")]
        public string QrImage { get; set; } = string.Empty;

        [BsonElement("wardenId")]
        public string WardenId { get; set; } = string.Empty;

        [BsonElement("session")]
        public string Session { get; set; } = string.Empty; // morning / evening

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}

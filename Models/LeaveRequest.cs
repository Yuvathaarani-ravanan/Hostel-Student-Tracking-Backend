using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HostelStudentTracking.Models
{
    public class LeaveRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("studentName")]
        public string StudentName { get; set; } = "";

        [BsonElement("rollNo")]
        public string RollNo { get; set; } = "";

        [BsonElement("fromDate")]
        public DateTime FromDate { get; set; }

        [BsonElement("toDate")]
        public DateTime ToDate { get; set; }

        [BsonElement("leaveType")]
        public string LeaveType { get; set; } = "";

        [BsonElement("reason")]
        public string Reason { get; set; } = "";

        [BsonElement("contactNumber")]
        public string ContactNumber { get; set; } = "";

        [BsonElement("status")]
        public string Status { get; set; } = "Pending";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

namespace HostelStudentTracking.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string WardenCollectionName { get; set; } = "Wardens"; 
        public string StudentCollectionName { get; set; } = "Students"; 
        
    }
}

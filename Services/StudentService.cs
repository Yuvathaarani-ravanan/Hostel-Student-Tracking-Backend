using MongoDB.Driver;
using HostelStudentTracking.Models;

namespace HostelStudentTracking.Services
{
    public class StudentService : IStudentService
    {
        private readonly IMongoCollection<Student> _students;

        public StudentService(IMongoDatabase database)
        {
            // CORRECTED: Changed from "students" to "student" to match your MongoDB collection
            _students = database.GetCollection<Student>("students");
            
            // Create unique indexes
            var indexKeysDefinition = Builders<Student>.IndexKeys
                .Ascending(s => s.RollNo)
                .Ascending(s => s.Email);
            
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Student>(indexKeysDefinition, indexOptions);
            
            _students.Indexes.CreateOne(indexModel);
        }

        public async Task<List<Student>> GetAsync() =>
            await _students.Find(_ => true).ToListAsync();

        public async Task<Student?> GetAsync(string id) =>
            await _students.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Student?> GetByRollNoAsync(string rollNo) =>
            await _students.Find(x => x.RollNo == rollNo).FirstOrDefaultAsync();

        public async Task<Student?> GetByEmailAsync(string email) =>
            await _students.Find(x => x.Email == email).FirstOrDefaultAsync();

        public async Task<Student?> GetByNameAsync(string name) =>
            await _students.Find(x => x.Name == name).FirstOrDefaultAsync();


        public async Task<Student> CreateAsync(CreateStudentRequest studentRequest)
        {
            // Check if roll number or email already exists
            var existingStudent = await GetByRollNoAsync(studentRequest.RollNo) ?? 
                                await GetByEmailAsync(studentRequest.Email) ??
                                await GetByNameAsync(studentRequest.Name);
            
            if (existingStudent != null)
            {
                throw new InvalidOperationException("Student with same Roll Number or Email or Name already exists");
            }

            var student = new Student
            {
                Name = studentRequest.Name,
                Email = studentRequest.Email,
                Mobile = studentRequest.Mobile,
                Department = studentRequest.Department,
                Year = studentRequest.Year,
                RollNo = studentRequest.RollNo,
                RoomNo = studentRequest.RoomNo,
                Password = studentRequest.Password,
                Address = studentRequest.Address,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _students.InsertOneAsync(student);
            return student;
        }

        public async Task UpdateAsync(string id, CreateStudentRequest studentIn)
        {
            var student = new Student
            {
                Id = id,
                Name = studentIn.Name,
                Email = studentIn.Email,
                Mobile = studentIn.Mobile,
                Department = studentIn.Department,
                Year = studentIn.Year,
                RollNo = studentIn.RollNo,
                RoomNo = studentIn.RoomNo,
                Password = studentIn.Password,
                Address = studentIn.Address,
                UpdatedAt = DateTime.UtcNow
            };

            await _students.ReplaceOneAsync(x => x.Id == id, student);
        }

        public async Task RemoveAsync(string id) =>
            await _students.DeleteOneAsync(x => x.Id == id);
            
        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            var students = await _students.Find(_ => true).ToListAsync();
            return students;
        }
    }
}
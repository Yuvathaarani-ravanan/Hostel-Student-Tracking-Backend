using HostelStudentTracking.Services;
using HostelStudentTracking.Hubs;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// ✅ Add SignalR for real-time updates
builder.Services.AddSignalR();

// ✅ MongoDB Configuration
var connectionString = builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")
                       ?? "mongodb://localhost:27017";
var databaseName = builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")
                   ?? "HostelStudentTrackingDB";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
builder.Services.AddSingleton<LeaveService>();

// ✅ Register Services (Scoped for each request)
builder.Services.AddScoped<IWardenService, WardenService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<QrService>();
builder.Services.AddScoped<AttendanceService>();

// ✅ QR Cleanup Service (Keep as is - don't change)
builder.Services.AddHostedService<QrCleanupService>();

// ✅ SIMPLE CORS FIX - No credentials with wildcard
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        // List specific origins (no wildcard with credentials)
        builder.WithOrigins(
                "http://localhost:8000",  // Django
                "http://127.0.0.1:8000",
                "http://localhost:3000",  // React
                "http://127.0.0.1:3000",
                "http://localhost:5500",  // Live Server
                "http://127.0.0.1:5500")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
    
    // SignalR specific policy (if needed)
    options.AddPolicy("SignalR", builder =>
    {
        builder.WithOrigins("http://localhost:8000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

// ✅ Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Use CORS (without credentials for regular API)
app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

// ✅ Map Controllers
app.MapControllers();

// ✅ Map SignalR Hub
app.MapHub<AttendanceHub>("/attendanceHub");

// ✅ Root endpoint
app.MapGet("/", () =>
    "🚀 Hostel Student Tracking API is running!\n✅ Swagger → /swagger\n✅ WebSocket Hub → ws://localhost:5071/attendanceHub");

// ✅ Start the app
app.Run();
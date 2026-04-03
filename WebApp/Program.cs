using AutoPartsSystem.Data;
using AutoPartsSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database connection string
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "autoparts.db");
var connectionString = $"Data Source={dbPath}";

// Register DI services using modern syntax
builder.Services.AddSingleton<IRepository>(new SqliteRepository(connectionString));
builder.Services.AddScoped<IdentityService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<AdminService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
DbInitializer.Initialize(connectionString);

app.UseStaticFiles();
app.UseCors();
app.UseSession();
app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();

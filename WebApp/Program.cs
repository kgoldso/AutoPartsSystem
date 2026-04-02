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

// Register DI services
builder.Services.AddSingleton<IRepository>(new SqliteRepository(connectionString));
builder.Services.AddScoped<IdentityService>(sp => new IdentityService(sp.GetRequiredService<IRepository>()));
builder.Services.AddScoped<OrderService>(sp => new OrderService(sp.GetRequiredService<IRepository>()));
builder.Services.AddScoped<WarehouseService>(sp =>
    new WarehouseService(sp.GetRequiredService<IRepository>(), sp.GetRequiredService<IdentityService>()));
builder.Services.AddScoped<AdminService>(sp =>
    new AdminService(sp.GetRequiredService<IRepository>(), sp.GetRequiredService<IdentityService>()));

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

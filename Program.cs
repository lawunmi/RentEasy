using Amazon.S3;
using Amazon;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RentEasy.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); 

// Load environment variables
DotNetEnv.Env.Load();

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    return new AmazonS3Client(
        Environment.GetEnvironmentVariable("MYACCESSKEY"),
        Environment.GetEnvironmentVariable("MYSECRETKEY"),
        RegionEndpoint.USEast1  
    );
});

builder.Services.AddDbContext<RentEasyContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Connection2RentEasyDB");

    // Fetch environment variables safely
    var dataSource = Environment.GetEnvironmentVariable("MYDATASOURCE") ?? "default_datasource";
    var database = Environment.GetEnvironmentVariable("MYDATABASE") ?? "default_database";
    var userId = Environment.GetEnvironmentVariable("MYUSERID") ?? "default_user";
    var password = Environment.GetEnvironmentVariable("MYPASSWORD") ?? "default_password";

    // Replace placeholders with actual values
    connectionString = connectionString
        .Replace("MYDATASOURCE", dataSource)
        .Replace("MYDATABASE", database)
        .Replace("MYUSERID", userId)
        .Replace("MYPASSWORD", password);

    Console.WriteLine($"[DEBUG] Connection String: {connectionString}"); // Debugging

    options.UseSqlServer(connectionString);
});

// --- Configure cookie authentication ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

// --- Add Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RentEasy API",
        Version = "v1",
        Description = "API documentation for RentEasy application."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RentEasy API v1");
    });
}

// --- Map Routes ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure API controllers are mapped
app.MapControllers();

app.Run();

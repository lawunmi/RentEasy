using Microsoft.EntityFrameworkCore;
using RentEasy.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

DotNetEnv.Env.Load();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Microsoft.EntityFrameworkCore;
using RentEasy.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RentEasyContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Connection2RentEasyDB");

    // Replace placeholders with environment variables
    connectionString = connectionString
        .Replace("MYDATASOURCE", Environment.GetEnvironmentVariable("MYDATASOURCE"))
        .Replace("MYDATABASE", Environment.GetEnvironmentVariable("MYDATABASE"))
        .Replace("MYUSERID", Environment.GetEnvironmentVariable("MYUSERID"))
        .Replace("MYPASSWORD", Environment.GetEnvironmentVariable("MYPASSWORD"));

    options.UseSqlServer(connectionString);
});

DotNetEnv.Env.Load(); 
builder.Services.AddDbContext<RentEasyContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Connection2RentEasyDB");
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

using ESD_Project.Data;
using ESD_Project.Models;
using ESD_Project.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Register EF Core
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Register your BackgroundService
builder.Services.AddHostedService<IoTHubReceiverService>();
builder.Services.AddHostedService<AlertProcessingService>();

// 3) Register MVC
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<EnergyUsageService>();

builder.Services.AddScoped<LoadMonitoringService>();

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();



// Now build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Serve static files if you need them
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

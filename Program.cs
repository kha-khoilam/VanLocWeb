using Microsoft.EntityFrameworkCore;
using VanLocWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
}
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback to avoid "ConnectionString not initialized" error locally
    connectionString = "Host=localhost;Database=dummy;Username=dummy;Password=dummy";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<VanLocWeb.Services.DataService>();
builder.Services.AddSingleton<VanLocWeb.Services.PdfService>();

// Add Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<VanLocWeb.Middleware.VisitTrackerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dataService = scope.ServiceProvider.GetRequiredService<VanLocWeb.Services.DataService>();
        dataService.InitializeDatabase();
    }
    catch (Exception ex)
    {
        // Log error but allow app to start so user can still see local changes
        Console.WriteLine("Database initialization failed: " + ex.Message);
        Console.WriteLine("This is expected if you don't have PostgreSQL installed locally. The app will still start.");
    }
}

app.Run();

using System.Security.Claims;
using System.IO;
using GPH.Data;
using GPH.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// ✅ DATA PROTECTION — Persist keys across app restarts
// ======================================================
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "App_Data", "Keys")))
    .SetApplicationName("GPHApp");

// ======================================================
// ✅ CORS Policy
// ======================================================
var MyAllowSpecificOrigins = "MyAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000", // Local development
            "https://gupta-publishing-house.vercel.app",
            "https://gph.indusanalytics.co.in"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ======================================================
// ✅ Database Connection (SQL Server)
// ======================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ======================================================
// ✅ Authentication & Cookies
// ======================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IDailyAllowanceService, DailyAllowanceService>();
builder.Services.AddScoped<IExcelParserService, ExcelParserService>();
builder.Services.AddHttpClient<IGeocodingService, GoogleGeocodingService>();

builder.Services.AddControllers();

// ======================================================
// ✅ Large File Upload Support
// ======================================================
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

// ======================================================
// ✅ Build and Middleware Pipeline
// ======================================================
var app = builder.Build();
// --- ADD THIS MIDDLEWARE (ORDER IS IMPORTANT) ---
// It should be placed after UseRouting and before UseEndpoints (or MapControllers)
app.UseHttpMethodOverride();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

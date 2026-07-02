using EmployeePortal.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Service registrations ─────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Scoped = one instance per HTTP request (correct for stateless API calls to BC)
builder.Services.AddScoped<IBcService, BcService>();

// Singleton = one instance for the app lifetime (stateless, safe to share)
builder.Services.AddSingleton<IPasswordService, PasswordService>();

// ── CORS ──────────────────────────────────────────────────────────────────────
// Allows the Next.js frontend on port 3000 to call this API during development.
// Restrict this to your production domain before going live.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextjs",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ── App pipeline ──────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseCors("AllowNextjs");
app.MapControllers();

app.Run();
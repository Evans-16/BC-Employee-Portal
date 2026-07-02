using EmployeePortal.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC Controllers
builder.Services.AddControllers();

// Register HttpClient factory
builder.Services.AddHttpClient();

// Register our Business Central service (scoped = one per request)
builder.Services.AddScoped<IBcService, BcService>();

// Enable CORS for the Next.js frontend on port 3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextjs",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowNextjs");

// Map all [ApiController] routes automatically
app.MapControllers();

app.Run();
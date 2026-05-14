using System.Security.Claims;
using System.Text;
using MediTrack.Api.Data; // lets program.cs recognize ApplicationDbContext
using MediTrack.Api.Models;
using MediTrack.Api.Services; // lets program.cs recognize PatientService
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // lets program.cs use EF core methods
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// This tells Asp.Net core that we are using controller classes
// Later, this allows routes like /api/patients or /api/appointments to work
builder.Services.AddControllers();

// This registers our database context with the app.
// ApplicationDbContext is our bridge between C# models and the database.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // This reads "DefaultConnection" from appsettings.Development.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // This tells if the SQL connection fails temporarily, retry instead of immediately failing.
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }
    );
});

// AddIdentity<ApplicationUser, IdentityRole means use our ApplicationUser class for users, and use built-in IdentityRole for roles like Admin, Provider, Patient.
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // These settings are relazed for learning
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // means store users, roles, passwords, and user-role relationships in our Azure SQL database through EF Core.
    .AddDefaultTokenProviders();

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];

if (
    string.IsNullOrWhiteSpace(jwtIssuer)
    || string.IsNullOrWhiteSpace(jwtAudience)
    || string.IsNullOrWhiteSpace(jwtKey)
)
{
    throw new InvalidOperationException("JWT settings are missing");
}

// This means, When a request has Authorization: Bearer <token>,
// validate the token using our issuer, audience, expiry, and signing key.
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };
    });
builder.Services.AddAuthorization();

// this tells .NET, if a controller asks for PatientService, create one for that request
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<ProviderService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();

// This builds the actual web application
// Before this line, we are only registering/configuring services
// After this line, we configure how request move through the app
var app = builder.Build();

// Simple test route so we can quickly check if the API is alive
// When we visit http://localhost:5126/, it should show this message
app.MapGet("/", () => "MediTrack API is running");

app.UseAuthentication(); // This checks if the request has a valid JWT token, and sets the user for that request
app.UseAuthorization(); // This checks if the user is allowed to access the endpoint

// This connects controller routes to the app
// Without this, controller endpoints will not work
app.MapControllers();

// This starts the web server
app.Run();

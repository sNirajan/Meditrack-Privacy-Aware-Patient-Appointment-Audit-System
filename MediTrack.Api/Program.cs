using MediTrack.Api.Data; // lets program.cs recognize ApplicationDbContext
using Microsoft.EntityFrameworkCore; // lets program.cs use EF core methods 

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

    // This tells EF core to use SQLite as the database for local development
    options.UseSqlServer(connectionString);
});

// This builds the actual web application
// Before this line, we are only registering/configuring services
// After this line, we configure how request move through the app
var app = builder.Build();

// Simple test route so we can quickly check if the API is alive
// When we visit http://localhost:5126/, it should show this message
app.MapGet("/", () => "MediTrack API is running");

// This connects controller routes to the app
// Without this, controller endpoints will not work
app.MapControllers();

// This starts the web server
app.Run();





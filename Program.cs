using ECommerceApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

if (builder.Environment.EnvironmentName != "Testing")
{
    await app.MigrateDatabaseAndSeedData();
}
app.UseCustomPipeline();

app.Run();


public partial class Program { }
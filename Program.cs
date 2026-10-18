using ECommerceApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();
await app.MigrateDatabaseAndSeedData();
app.UseCustomPipeline();

app.Run();

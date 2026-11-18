using ECommerceApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Extensions
{
    public static class InitializeDatabaseExtensions
    {
        public static async Task<WebApplication> MigrateDatabaseAndSeedData(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<AppDbContext>>();
             

               
                var context = services.GetRequiredService<AppDbContext>();

                logger.LogInformation("Applying pending migrations for database context {ContextName}...", typeof(AppDbContext).Name);

                // This applies any pending migrations and creates the DB/tables if they don't exist
                await context.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
                
                logger.LogInformation("Database migration completed successfully.");

                await DataSeeder.SeedAdminDataAsync(services, app.Lifetime.ApplicationStopping);



            }
            return app;
        }
    }
}

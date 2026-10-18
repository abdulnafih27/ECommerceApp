using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Data
{
    public static class DataSeeder
    { 
        public static async Task SeedAdminDataAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
                

                // Get the AppDbContext from the service provider
                var dbContext = services.GetRequiredService<AppDbContext>();
          
                
                // Check if the admin user already exists
                if (!await dbContext.Users.AnyAsync(u => u.Role == Roles.Admin, cancellationToken))
                {
                    var logger = services.GetRequiredService<ILogger<AppDbContext>>();
                    var configuration = services.GetRequiredService<IConfiguration>();
                    var hasher = services.GetRequiredService<IPasswordHasher>();

                    // Get the user data from the configuration
                    var email = configuration["AdminUser:Email"] ?? throw new InvalidOperationException("Admin Email not found in the configuration");
                    var username = configuration["AdminUser:Username"] ?? throw new InvalidOperationException("Admin Username not found in the configuration");
                    var password = configuration["AdminUser:Password"] ?? throw new InvalidOperationException("Admin Password not found in the configuration");

                    // Check if the admin with username or email already there
                    var usernameTaken = await dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == username.ToLowerInvariant(), cancellationToken);
                    if (usernameTaken)
                    {
                        throw new InvalidOperationException($"Admin seeding username {username} already exists in the database");
                    }

                    var emailTaken = await dbContext.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
                    if (emailTaken)
                    {
                        throw new InvalidOperationException($"Admin seeding email {email} already exists in the database");
                    }

                    var adminUser = new User
                    {
                        Username = username,
                        PasswordHash = hasher.HashPassword(password),
                        Email = email,
                        Role = Roles.Admin
                    
                    };

                    await dbContext.Users.AddAsync(adminUser, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("Admin data successfully added to the Database on initialization");
                }
           
        }
    }
}

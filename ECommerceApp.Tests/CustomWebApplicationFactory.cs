

using ECommerceApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace ECommerceApp.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=localhost\\SQLEXPRESS;Database=ECommerce_Test;Trusted_Connection=True;TrustServerCertificate=True;");

            
            builder.UseSetting("Jwt:Key", "SuperSecretTestingKeyThatIsAtLeast32CharactersLong!");
            builder.UseSetting("Jwt:Issuer", "TestIssuer");
            builder.UseSetting("Jwt:Audience", "TestAudience");
            builder.UseSetting("Jwt:ExpiryInMinutes", "60");


            builder.UseSetting("AdminUser:Email", "admin@test.com");
            builder.UseSetting("AdminUser:Username", "admin");
            builder.UseSetting("AdminUser:Password", "SecureTestPass123!");


            base.ConfigureWebHost(builder);
        }

        public async Task ResetDatabase(CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await db.Database.MigrateAsync(cancellationToken);

            await db.Orders.IgnoreQueryFilters()
                .ExecuteDeleteAsync(cancellationToken);

            await db.CartItems.IgnoreQueryFilters()
                .ExecuteDeleteAsync(cancellationToken);

            await db.Products.IgnoreQueryFilters()
                .ExecuteDeleteAsync(cancellationToken);

            await db.Categories.IgnoreQueryFilters()
                .ExecuteDeleteAsync(cancellationToken);

            await db.Users.IgnoreQueryFilters().Where(u => u.Role != Roles.Admin).ExecuteDeleteAsync(cancellationToken);

            if (!await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Role == Roles.Admin, cancellationToken))
            {
                var user = new User
                {
                    Username = "admin",
                    Email = "admin@test.com",
                    PasswordHash = passwordHasher.HashPassword("admin@123"),
                    Role = Roles.Admin
                };

                await db.Users.AddAsync(user, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

        }

        public async Task<(int UserId, string AccessToken)> SeedUserAsync(string username, string email, string password, CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

      
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHasher.HashPassword(password)
            };

            await db.Users.AddAsync(user, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var token = tokenService.GenerateToken(user);

            return (user.UserId, token);
        }
        public async Task<string> LoginAsAdminAsync(CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Role == Roles.Admin, cancellationToken);

            if (user == null)
                throw new InvalidOperationException("Admin user was not found.");

            return tokenService.GenerateToken(user);
        }

        public async Task<int> SeedCategoryAsync(CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


            var category = new Category
            {
                CategoryName = $"TestCategory_{Guid.NewGuid():N}",
                CategoryDescription = "Category created for automated integration testing."
            };

            await db.Categories.AddAsync(category, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return category.CategoryId;
        }
        
        public async Task<int> SeedProductAsync(int categoryId, int stock, decimal price, CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var product = new Product
            {
                ProductName = $"TestProduct_{Guid.NewGuid():N}",
                ProductDescription = "Product created for automated integration testing.",
                CategoryId = categoryId,
                StockQuantity = stock,
                Price = price,
                ImageUrl = "https://example.com/test-product.jpg"
            };

            await db.Products.AddAsync(product, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return product.ProductId;
        }

        public async Task SeedCartItemAsync(int userId, int productId, int quantity, CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            };

            await db.CartItems.AddAsync(cartItem, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Product?> GetProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await db.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        public async Task<int> GetCartItemCountAsync(int userId, CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await db.CartItems
                .IgnoreQueryFilters()
                .CountAsync(c => c.UserId == userId, cancellationToken);
        }

        public async Task<int> GetOrderCountAsync(int userId)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.Orders
                .IgnoreQueryFilters()
                .CountAsync(o => o.UserId == userId);
        }

        public async Task RetireProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var product = await context.Products
                                        .IgnoreQueryFilters()
                                        .FirstOrDefaultAsync(o => o.ProductId == productId);

            if (product == null) {
                throw new InvalidOperationException("Product was not found.");
            }

            product.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);
        }

    }
}

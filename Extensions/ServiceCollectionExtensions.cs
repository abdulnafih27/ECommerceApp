using ECommerceApp.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

        services.AddControllers();
        services.AddOpenApi();
        services.AddAuthorization();

        // Create sql connection
        var connectionString =  config.GetConnectionString("DefaultConnection");
        //services.AddDbContext<AppDbContext>(options => 
        //            options.UseSqlite(connectionString));

        services.AddDbContext<AppDbContext>(options =>
                         options.UseSqlServer(connectionString));

        // AutoMapper service registration
        services.AddAutoMapper(cfg =>
        {
           cfg.AddProfile<UserMappingProfile>();
           cfg.AddProfile<ProductMappingProfile>();
           cfg.AddProfile<CategoryMappingProfile>();
           cfg.AddProfile<CartItemsMappingProfile>();
        }, typeof(UserMappingProfile), typeof(ProductMappingProfile), typeof(CategoryMappingProfile), typeof(CartItemsMappingProfile));

        // Register JWT Authentication 
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not found in the configuration");
        var issuer = config["Jwt:Issuer"]  ?? throw new InvalidOperationException("JWT Issuer not found in the configuration");
        var audience = config["Jwt:Audience"]  ?? throw new InvalidOperationException("JWT Audience not found in the configuration");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => 
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            });
        
        // Adding Json serilization to service
        services.AddControllers()
                .AddJsonOptions(options =>
                { 
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        // Configure ProblemDetails to give the TraceId
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                string traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                context.ProblemDetails.Extensions["traceId"] = traceId;

                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
            };
        });

        // Adding a Authorization Policy
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ActiveStatus", policy =>
            {
               policy.Requirements.Add(new ActiveStatusRequirement()); 
            });
        });


        // Adding token and password as singleton life time since its stateless and does one job
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Adding Authorization Handler to the DI as scoped since it uses AppDbContext
        services.AddScoped<IAuthorizationHandler, ActiveStatusAuthorizationHandler>();

        // Adding Controller Services as Scoped since its required in that http scope
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
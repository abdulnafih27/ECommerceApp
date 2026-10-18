using ECommerceApp.Data;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<User> Users {get; set;} 

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var entries = ChangeTracker.Entries<IAuditable>()
                        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        var utcNow = DateTime.UtcNow;

        foreach(var entry in entries)
        {
            entry.Entity.ModifiedOn = utcNow;

            if(entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = utcNow;
            }    
        }

        // Make the email to the lowercase before updating to the db
        var userEntries = ChangeTracker.Entries<User>()
                            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var user in userEntries)
        {
            user.Entity.Email = user.Entity.Email.ToLowerInvariant();
            user.Entity.Username = user.Entity.Username.ToLowerInvariant();
        }

        return await base.SaveChangesAsync(cancellationToken);

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Add the Date converter to the configuration
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
        
        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }
}
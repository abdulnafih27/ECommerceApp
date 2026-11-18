using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(60);
        
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
        
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
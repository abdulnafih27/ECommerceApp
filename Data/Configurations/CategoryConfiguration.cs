using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {

        builder.HasKey(c => c.CategoryId);
        builder.HasIndex(c => c.CategoryName).IsUnique();

        builder.Property(c => c.CategoryName)
                .HasMaxLength(100);
        builder.Property(c => c.CategoryDescription)
                .HasMaxLength(2000);
    }
}
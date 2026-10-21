using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {

        builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductName)
                .HasMaxLength(200);
        builder.Property(p => p.ProductDescription)
                .HasMaxLength(2000);
        builder.Property(p => p.ImageUrl)
                .HasMaxLength(2048);

        builder.Property(p => p.Price)
                .HasPrecision(18, 2);
        
        builder.Property(p => p.RowVersion)
                .IsRowVersion();


        builder.ToTable("Products", t =>
        {
            t.HasCheckConstraint("CK_Products_Price", "[Price] > 0");
            t.HasCheckConstraint("CK_Products_StockQuantity", "[StockQuantity] >= 0");
        });
        
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
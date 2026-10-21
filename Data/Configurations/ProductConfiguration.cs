using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {

        builder.HasOne(c => c.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(c => c.CategoryId)
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


        builder.ToTable(p => p.HasCheckConstraint("CK_Product_Price", "[Price] > 0"));
        builder.ToTable(p => p.HasCheckConstraint("CK_Product_StockQuantity", "[StockQuantity] > 0"));
        
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
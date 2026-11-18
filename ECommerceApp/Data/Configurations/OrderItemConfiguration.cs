using ECommerceApp.Common;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApp.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(oi => oi.OrderItemId);

            builder.HasIndex(oi => new { oi.OrderId, oi.ProductId }).IsUnique();

            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(oi => oi.ProductName)
                .HasMaxLength(ProductLimits.NameMaxLength)
                .IsRequired();

            builder.ToTable("OrderItems", oi =>
            {
                oi.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] >= 0");
                oi.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
            }
            );

        }
    }
}

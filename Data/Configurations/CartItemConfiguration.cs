using ECommerceApp.Common;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApp.Data.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(ci => ci.CartItemId);

            builder.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();

            builder.HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(ci => ci.HasCheckConstraint("CK_CartItems_Quantity", $"[Quantity] > 0 AND [Quantity] <= {CartLimits.MaxQuantityPerItem}"));
        }
    }
}

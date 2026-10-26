using ECommerceApp.Common;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApp.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.OrderId);

            builder.HasIndex(o => new { o.UserId, o.CreatedOn });
            
            builder.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.ShippingFullName)
                .HasMaxLength(OrderLimits.ShippingFullNameMaxLength)
                .IsRequired();

            builder.Property(o => o.ShippingPhone)
                .HasMaxLength(OrderLimits.ShippingPhoneMaxLength)
                .IsRequired();

            builder.Property(o => o.ShippingAddressLine)
                .HasMaxLength(OrderLimits.ShippingAddressLineMaxLength)
                .IsRequired();

            builder.Property(o => o.ShippingCity)
                .HasMaxLength(OrderLimits.ShippingCityMaxLength)
                .IsRequired();

            builder.Property(o => o.ShippingState)
                .HasMaxLength(OrderLimits.ShippingStateMaxLength)
                .IsRequired();

            builder.Property(o => o.ShippingPostalCode)
                .HasMaxLength(OrderLimits.ShippingPostalCodeMaxLength)
                .IsRequired();
        }
    }
}

using bagsisbaku.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations.Orders;

internal sealed class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(
        EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "order_items",
            "sales");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired();

        builder.Property(item => item.ProductVariantId)
            .IsRequired();

        builder.HasIndex(item => item.OrderId);

        builder.HasIndex(item => item.ProductId);

        builder.HasIndex(item => item.ProductVariantId);

        builder.Property(item => item.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.ProductCode)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.ProductType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.BrandName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(item => item.SizeId)
            .IsRequired();

        builder.Property(item => item.Size)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.ColorId)
            .IsRequired();

        builder.Property(item => item.Color)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.HexCode)
            .HasMaxLength(16);

        builder.Property(item => item.OriginalUnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Ignore(
            item => item.UnitDiscountAmount);

        builder.Ignore(
            item => item.LineSubtotal);

        builder.Ignore(
            item => item.LineDiscountAmount);

        builder.Ignore(
            item => item.LineTotal);
    }
}
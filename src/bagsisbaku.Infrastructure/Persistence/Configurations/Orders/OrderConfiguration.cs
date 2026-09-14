using bagsisbaku.Domain.Orders;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations.Orders;

internal sealed class OrderConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(
            "orders",
            "sales");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.UserId)
            .IsRequired();

        builder.Property(order => order.OrderNumber)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(order => order.OrderNumber)
            .IsUnique();

        builder.HasIndex(
            order => new
            {
                order.UserId,
                order.PlacedAtUtc
            });

        builder.HasIndex(
            order => new
            {
                order.Status,
                order.PlacedAtUtc
            });

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.DeliveryType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.PaymentMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.RecipientFullName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(order => order.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(order => order.City)
            .HasMaxLength(120);

        builder.Property(order => order.District)
            .HasMaxLength(120);

        builder.Property(order => order.AddressLine)
            .HasMaxLength(500);

        builder.Property(order => order.PostalCode)
            .HasMaxLength(32);

        builder.Property(order => order.DeliveryNote)
            .HasMaxLength(1000);

        builder.Property(order => order.CustomerNote)
            .HasMaxLength(1000);

        builder.Property(order => order.Latitude)
            .HasPrecision(9, 6);

        builder.Property(order => order.Longitude)
            .HasPrecision(9, 6);

        builder.Property(order => order.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.DeliveryFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.PlacedAtUtc)
            .IsRequired();

        builder.Property(order => order.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Ignore(order => order.CanBeCancelled);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        builder.HasMany(order => order.StatusHistory)
            .WithOne()
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.StatusHistory)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}
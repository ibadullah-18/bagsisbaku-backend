using bagsisbaku.Domain.Customers;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

public sealed class CustomerAddressConfiguration
    : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(
        EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable(
            "customer_addresses",
            "customers");

        builder.HasKey(address => address.Id);

        builder.Property(address => address.UserId)
            .IsRequired();

        builder.Property(address => address.Title)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(
                address => address.RecipientFullName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(address => address.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(address => address.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(address => address.District)
            .HasMaxLength(120);

        builder.Property(address => address.AddressLine)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(address => address.PostalCode)
            .HasMaxLength(20);

        builder.Property(address => address.DeliveryNote)
            .HasMaxLength(500);

        builder.Property(address => address.Latitude)
            .HasPrecision(9, 6);

        builder.Property(address => address.Longitude)
            .HasPrecision(9, 6);

        builder.Property(address => address.IsDefault)
            .IsRequired();

        builder.HasIndex(address => address.UserId)
            .HasDatabaseName(
                "ix_customer_addresses_user_id");

        builder.HasIndex(
                address => new
                {
                    address.UserId,
                    address.IsDefault
                })
            .IsUnique()
            .HasFilter("[IsDefault] = 1")
            .HasDatabaseName(
                "ux_customer_addresses_user_default");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(address => address.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
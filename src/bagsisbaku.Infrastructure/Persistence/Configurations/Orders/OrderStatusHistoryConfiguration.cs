using bagsisbaku.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations.Orders;

internal sealed class OrderStatusHistoryConfiguration
    : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(
        EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable(
            "order_status_history",
            "sales");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id)
            .ValueGeneratedNever();

        builder.Property(history => history.OrderId)
            .IsRequired();

        builder.HasIndex(
            history => new
            {
                history.OrderId,
                history.ChangedAtUtc
            });

        builder.Property(history => history.PreviousStatus)
            .HasConversion<int?>();

        builder.Property(history => history.NewStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.Note)
            .HasMaxLength(500);

        builder.Property(history => history.ChangedAtUtc)
            .IsRequired();
    }
}
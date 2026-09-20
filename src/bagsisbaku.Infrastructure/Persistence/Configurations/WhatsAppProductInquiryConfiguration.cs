using bagsisbaku.Domain.Engagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class WhatsAppProductInquiryConfiguration
    : IEntityTypeConfiguration<WhatsAppProductInquiry>
{
    public void Configure(
        EntityTypeBuilder<WhatsAppProductInquiry> builder)
    {
        builder.ToTable(
            "whatsapp_product_inquiries",
            "engagement");

        builder.ConfigureAuditable();

        builder.Property(inquiry => inquiry.ProductId)
            .IsRequired();

        builder.Property(inquiry => inquiry.ProductCode)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(inquiry => new
        {
            inquiry.ProductId,
            inquiry.CreatedAtUtc
        });
    }
}
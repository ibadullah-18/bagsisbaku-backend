using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Engagement;

public sealed class WhatsAppProductInquiry : AuditableEntity
{
    private WhatsAppProductInquiry()
    {
    }

    private WhatsAppProductInquiry(
        Guid id,
        Guid productId,
        string productCode)
        : base(id)
    {
        ProductId =
            DomainGuard.NotEmpty(
                productId,
                nameof(ProductId));

        ProductCode =
            DomainGuard.Required(
                productCode,
                nameof(ProductCode),
                80);
    }

    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } =
        string.Empty;

    public static WhatsAppProductInquiry Create(
        Guid productId,
        string productCode)
    {
        return new WhatsAppProductInquiry(
            Guid.NewGuid(),
            productId,
            productCode);
    }
}
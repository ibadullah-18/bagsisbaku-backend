using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Engagement;

public sealed record WhatsAppProductInquiryModel(
    Guid InquiryId,
    Guid ProductId,
    string WhatsAppUrl);

public interface IWhatsAppProductInquiryService
{
    Task<Result<WhatsAppProductInquiryModel>> CreateAsync(
        Guid productId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}
using bagsisbaku.Application.Catalog.Products.Public;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Engagement;
using bagsisbaku.Application.Store;
using bagsisbaku.Domain.Engagement;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;

namespace bagsisbaku.Infrastructure.Engagement;

public sealed class WhatsAppProductInquiryService(
    IProductCatalogQuery productCatalogQuery,
    IStoreSettingsService storeSettingsService,
    ApplicationDbContext dbContext)
    : IWhatsAppProductInquiryService
{
    public async Task<Result<WhatsAppProductInquiryModel>>
        CreateAsync(
            Guid productId,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<WhatsAppProductInquiryModel>(
                Error.Validation(
                    "whatsapp.invalid-product-id",
                    "Məhsul ID-si düzgün deyil."));
        }

        var product =
            await productCatalogQuery.GetByIdAsync(
                productId,
                language,
                cancellationToken);

        if (product is null)
        {
            return Result.Failure<WhatsAppProductInquiryModel>(
                Error.NotFound(
                    "whatsapp.product-not-found",
                    "Məhsul tapılmadı və ya satışda deyil."));
        }

        var settingsResult =
            await storeSettingsService.GetPublicAsync(
                language,
                cancellationToken);

        if (settingsResult.IsFailure)
        {
            return Result.Failure<WhatsAppProductInquiryModel>(
                settingsResult.Error);
        }

        var phoneDigits =
            new string(
                settingsResult.Value.WhatsAppPhone
                    .Where(char.IsDigit)
                    .ToArray());

        if (phoneDigits.StartsWith(
            "00",
            StringComparison.Ordinal))
        {
            phoneDigits = phoneDigits[2..];
        }
        else if (phoneDigits.StartsWith(
            '0'))
        {
            phoneDigits = "994" + phoneDigits[1..];
        }

        if (phoneDigits.Length is < 8 or > 15)
        {
            return Result.Failure<WhatsAppProductInquiryModel>(
                Error.Validation(
                    "whatsapp.invalid-store-phone",
                    "Mağazanın WhatsApp nömrəsi düzgün deyil."));
        }

        var message =
            language switch
            {
                SupportedLanguage.Russian =>
                    $"Здравствуйте! Меня интересует товар " +
                    $"{product.Name} ({product.ProductCode}).",

                SupportedLanguage.English =>
                    $"Hello! I am interested in " +
                    $"{product.Name} ({product.ProductCode}).",

                _ =>
                    $"Salam! {product.Name} " +
                    $"({product.ProductCode}) məhsulu ilə " +
                    "maraqlanıram."
            };

        var whatsappUrl =
            $"https://wa.me/{phoneDigits}?text=" +
            Uri.EscapeDataString(message);

        var inquiry =
            WhatsAppProductInquiry.Create(
                product.Id,
                product.ProductCode);

        dbContext.WhatsAppProductInquiries.Add(
            inquiry);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            new WhatsAppProductInquiryModel(
                inquiry.Id,
                product.Id,
                whatsappUrl));
    }
}
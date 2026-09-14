using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Baskets;

public interface IBasketService
{
    Task<Result<BasketModel>> GetBasketAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<BasketModel>> AddItemAsync(
        AddBasketItemCommand command,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<BasketModel>> UpdateItemQuantityAsync(
        Guid basketItemId,
        UpdateBasketItemQuantityCommand command,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<BasketModel>> RemoveItemAsync(
        Guid basketItemId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<BasketModel>> ClearBasketAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}
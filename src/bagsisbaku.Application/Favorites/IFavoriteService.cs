using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Favorites;

public interface IFavoriteService
{
    Task<Result<FavoriteListModel>> GetAllAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<FavoriteProductModel>> AddAsync(
        Guid productId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}
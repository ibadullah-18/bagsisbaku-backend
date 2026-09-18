using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Favorites;
using bagsisbaku.Domain.Favorites;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Favorites;

internal sealed class FavoriteService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : IFavoriteService
{
    public async Task<Result<FavoriteListModel>> GetAllAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<FavoriteListModel>(
                userResult.Error);
        }

        var selectedLanguage =
            NormalizeLanguage(language);

        var items =
            await CreateFavoriteItemsQuery(
                    userResult.Value,
                    selectedLanguage)
                .ToArrayAsync(
                    cancellationToken);

        return Result.Success(
            new FavoriteListModel(
                ToLanguageCode(selectedLanguage),
                items.Length,
                items));
    }

    public async Task<Result<FavoriteProductModel>> AddAsync(
        Guid productId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<FavoriteProductModel>(
                FavoriteErrors.ProductNotFound);
        }

        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<FavoriteProductModel>(
                userResult.Error);
        }

        var userId =
            userResult.Value;

        var selectedLanguage =
            NormalizeLanguage(language);

        var existingItem =
            await CreateFavoriteItemsQuery(
                    userId,
                    selectedLanguage,
                    productId)
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (existingItem is not null)
        {
            return Result.Success(
                existingItem);
        }

        var productState =
            await (
                from product in
                    dbContext.Products.AsNoTracking()

                join brand in
                    dbContext.Brands.AsNoTracking()
                    on product.BrandId equals brand.Id

                join category in
                    dbContext.Categories.AsNoTracking()
                    on product.CategoryId equals category.Id

                where product.Id == productId

                select new
                {
                    ProductIsActive =
                        product.IsActive,

                    BrandIsActive =
                        brand.IsActive,

                    CategoryIsActive =
                        category.IsActive
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (productState is null)
        {
            return Result.Failure<FavoriteProductModel>(
                FavoriteErrors.ProductNotFound);
        }

        if (!productState.ProductIsActive ||
            !productState.BrandIsActive ||
            !productState.CategoryIsActive)
        {
            return Result.Failure<FavoriteProductModel>(
                FavoriteErrors.ProductUnavailable);
        }

        var favorite =
            Favorite.Create(
                userId,
                productId);

        await dbContext.Favorites.AddAsync(
            favorite,
            cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();

            var concurrentItem =
                await CreateFavoriteItemsQuery(
                        userId,
                        selectedLanguage,
                        productId)
                    .SingleOrDefaultAsync(
                        cancellationToken);

            if (concurrentItem is not null)
            {
                return Result.Success(
                    concurrentItem);
            }

            return Result.Failure<FavoriteProductModel>(
                FavoriteErrors.UnexpectedFailure);
        }

        var createdItem =
            await CreateFavoriteItemsQuery(
                    userId,
                    selectedLanguage,
                    productId)
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (createdItem is null)
        {
            return Result.Failure<FavoriteProductModel>(
                FavoriteErrors.UnexpectedFailure);
        }

        return Result.Success(
            createdItem);
    }

    public async Task<Result> RemoveAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure(
                userResult.Error);
        }

        if (productId == Guid.Empty)
        {
            return Result.Success();
        }

        var favorite =
            await dbContext.Favorites
                .SingleOrDefaultAsync(
                    item =>
                        item.UserId ==
                            userResult.Value &&
                        item.ProductId ==
                            productId,
                    cancellationToken);

        if (favorite is null)
        {
            return Result.Success();
        }

        dbContext.Favorites.Remove(
            favorite);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    private async Task<Result<Guid>>
        GetCurrentUserIdAsync(
            CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            return Result.Failure<Guid>(
                FavoriteErrors.AuthenticationRequired);
        }

        var user =
            await dbContext.Users
                .AsNoTracking()
                .Where(item =>
                    item.Id == userId)
                .Select(
                    item => new
                    {
                        item.IsActive
                    })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(
                FavoriteErrors.CustomerNotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure<Guid>(
                FavoriteErrors.CustomerInactive);
        }

        return Result.Success(
            userId);
    }

    private IQueryable<FavoriteProductModel>
        CreateFavoriteItemsQuery(
            Guid userId,
            SupportedLanguage language,
            Guid? productId = null)
    {
        return
            from favorite in
                dbContext.Favorites
                    .AsNoTracking()

            join product in
                dbContext.Products
                    .AsNoTracking()
                on favorite.ProductId
                equals product.Id

            join brand in
                dbContext.Brands
                    .AsNoTracking()
                on product.BrandId
                equals brand.Id

            join category in
                dbContext.Categories
                    .AsNoTracking()
                on product.CategoryId
                equals category.Id

            where
                favorite.UserId == userId &&
                (!productId.HasValue ||
                 favorite.ProductId ==
                    productId.Value)

            orderby favorite.CreatedAtUtc
                descending

            select new FavoriteProductModel(
                favorite.Id,
                product.Id,

                dbContext.ProductTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            translation.ProductId ==
                                product.Id &&
                            translation.Language ==
                                language)
                    .Select(
                        translation =>
                            translation.Name)
                    .FirstOrDefault()
                    ?? product.Name,

                product.ProductCode,
                product.Model,
                (int)product.ProductType,
                brand.Name,
                product.Price,
                product.DiscountPrice,
                product.DiscountPrice
                    ?? product.Price,
                product.DiscountPrice.HasValue,

                dbContext.ProductImages
                    .AsNoTracking()
                    .Where(
                        image =>
                            image.ProductId ==
                                product.Id)
                    .OrderByDescending(
                        image =>
                            image.IsPrimary)
                    .ThenBy(
                        image =>
                            image.SortOrder)
                    .Select(
                        image =>
                            image.ImageUrl)
                    .FirstOrDefault(),

                product.IsActive &&
                brand.IsActive &&
                category.IsActive &&
                dbContext.ProductVariants
                    .AsNoTracking()
                    .Any(
                        variant =>
                            variant.ProductId ==
                                product.Id &&
                            variant.IsActive &&
                            variant.StockCount > 0),

                favorite.CreatedAtUtc);
    }

    private static SupportedLanguage NormalizeLanguage(
        SupportedLanguage language)
    {
        return Enum.IsDefined(language)
            ? language
            : SupportedLanguage.Azerbaijani;
    }

    private static string ToLanguageCode(
        SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.Russian =>
                "ru",

            SupportedLanguage.English =>
                "en",

            _ =>
                "az"
        };
    }
}
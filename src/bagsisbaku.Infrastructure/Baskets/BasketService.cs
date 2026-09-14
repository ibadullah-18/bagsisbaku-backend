using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Baskets;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Baskets;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Baskets;

internal sealed class BasketService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    BasketModelBuilder modelBuilder)
    : IBasketService
{
    public async Task<Result<BasketModel>> GetBasketAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                userResult.Error);
        }

        var basket =
            await FindBasketAsync(
                userResult.Value,
                trackChanges: false,
                cancellationToken);

        var model =
            await modelBuilder.BuildAsync(
                basket,
                language,
                cancellationToken);

        return Result.Success(model);
    }

    public async Task<Result<BasketModel>> AddItemAsync(
        AddBasketItemCommand command,
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ProductVariantId == Guid.Empty)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.ProductVariantNotFound);
        }

        var quantityError =
            ValidateQuantity(command.Quantity);

        if (quantityError != Error.None)
        {
            return Result.Failure<BasketModel>(
                quantityError);
        }

        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                userResult.Error);
        }

        var userId = userResult.Value;

        var basket =
            await FindBasketAsync(
                userId,
                trackChanges: true,
                cancellationToken);

        var existingItem =
            basket?.Items.SingleOrDefault(
                item =>
                    item.ProductVariantId ==
                    command.ProductVariantId);

        var desiredQuantity =
            (existingItem?.Quantity ?? 0L) +
            command.Quantity;

        if (desiredQuantity >
            BasketItem.MaximumQuantity)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.InvalidQuantity(
                    $"Bir məhsuldan maksimum " +
                    $"{BasketItem.MaximumQuantity} ədəd " +
                    "səbətə əlavə etmək olar."));
        }

        var variantResult =
            await ValidateSellableVariantAsync(
                command.ProductVariantId,
                checked((int)desiredQuantity),
                cancellationToken);

        if (variantResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                variantResult.Error);
        }

        if (basket is null)
        {
            basket = Basket.Create(userId);

            await dbContext.Baskets.AddAsync(
                basket,
                cancellationToken);
        }

        try
        {
            basket.AddItem(
                command.ProductVariantId,
                command.Quantity);
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.InvalidQuantity(
                    exception.Message));
        }

        if (existingItem is null)
        {
            var addedItem =
                basket.Items.Single(
                    item =>
                        item.ProductVariantId ==
                        command.ProductVariantId);

            dbContext.Entry(addedItem).State =
                EntityState.Added;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        var model =
            await modelBuilder.BuildAsync(
                basket,
                language,
                cancellationToken);

        return Result.Success(model);
    }

    public async Task<Result<BasketModel>>
        UpdateItemQuantityAsync(
            Guid basketItemId,
            UpdateBasketItemQuantityCommand command,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (basketItemId == Guid.Empty)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.BasketItemNotFound);
        }

        var quantityError =
            ValidateQuantity(command.Quantity);

        if (quantityError != Error.None)
        {
            return Result.Failure<BasketModel>(
                quantityError);
        }

        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                userResult.Error);
        }

        var basket =
            await FindBasketAsync(
                userResult.Value,
                trackChanges: true,
                cancellationToken);

        if (basket is null)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.BasketItemNotFound);
        }

        var basketItem =
            basket.Items.SingleOrDefault(
                item => item.Id == basketItemId);

        if (basketItem is null)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.BasketItemNotFound);
        }

        var variantResult =
            await ValidateSellableVariantAsync(
                basketItem.ProductVariantId,
                command.Quantity,
                cancellationToken);

        if (variantResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                variantResult.Error);
        }

        try
        {
            basket.UpdateItemQuantity(
                basketItemId,
                command.Quantity);
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.InvalidQuantity(
                    exception.Message));
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        var model =
            await modelBuilder.BuildAsync(
                basket,
                language,
                cancellationToken);

        return Result.Success(model);
    }

    public async Task<Result<BasketModel>> RemoveItemAsync(
        Guid basketItemId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        if (basketItemId == Guid.Empty)
        {
            return Result.Failure<BasketModel>(
                BasketErrors.BasketItemNotFound);
        }

        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                userResult.Error);
        }

        var basket =
            await FindBasketAsync(
                userResult.Value,
                trackChanges: true,
                cancellationToken);

        if (basket is null ||
            basket.Items.All(
                item => item.Id != basketItemId))
        {
            return Result.Failure<BasketModel>(
                BasketErrors.BasketItemNotFound);
        }

        basket.RemoveItem(basketItemId);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        var model =
            await modelBuilder.BuildAsync(
                basket,
                language,
                cancellationToken);

        return Result.Success(model);
    }

    public async Task<Result<BasketModel>> ClearBasketAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        var userResult =
            await GetCurrentUserIdAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<BasketModel>(
                userResult.Error);
        }

        var basket =
            await FindBasketAsync(
                userResult.Value,
                trackChanges: true,
                cancellationToken);

        if (basket is null)
        {
            var emptyModel =
                await modelBuilder.BuildAsync(
                    null,
                    language,
                    cancellationToken);

            return Result.Success(emptyModel);
        }

        basket.Clear();

        await dbContext.SaveChangesAsync(
            cancellationToken);

        var model =
            await modelBuilder.BuildAsync(
                basket,
                language,
                cancellationToken);

        return Result.Success(model);
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
                BasketErrors.AuthenticationRequired);
        }

        var user =
            await dbContext.Users
                .AsNoTracking()
                .Where(item => item.Id == userId)
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
                BasketErrors.CustomerNotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure<Guid>(
                BasketErrors.CustomerInactive);
        }

        return Result.Success(userId);
    }

    private async Task<Basket?> FindBasketAsync(
        Guid userId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Basket> query =
            dbContext.Baskets
                .Include(basket => basket.Items);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            basket => basket.UserId == userId,
            cancellationToken);
    }

    private async Task<Result<ProductVariant>>
        ValidateSellableVariantAsync(
            Guid productVariantId,
            int desiredQuantity,
            CancellationToken cancellationToken)
    {
        var variant =
            await dbContext.ProductVariants
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == productVariantId,
                    cancellationToken);

        if (variant is null)
        {
            return Result.Failure<ProductVariant>(
                BasketErrors.ProductVariantNotFound);
        }

        var product =
            await dbContext.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == variant.ProductId,
                    cancellationToken);

        if (product is null ||
            !product.IsActive)
        {
            return Result.Failure<ProductVariant>(
                BasketErrors.ProductUnavailable);
        }

        if (!variant.IsActive)
        {
            return Result.Failure<ProductVariant>(
                BasketErrors.ProductVariantUnavailable);
        }

        var sizeIsActive =
            await dbContext.Sizes
                .AsNoTracking()
                .Where(item => item.Id == variant.SizeId)
                .Select(item => (bool?)item.IsActive)
                .SingleOrDefaultAsync(
                    cancellationToken);

        var colorIsActive =
            await dbContext.Colors
                .AsNoTracking()
                .Where(item => item.Id == variant.ColorId)
                .Select(item => (bool?)item.IsActive)
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (sizeIsActive != true ||
            colorIsActive != true)
        {
            return Result.Failure<ProductVariant>(
                BasketErrors.ProductVariantUnavailable);
        }

        if (variant.StockCount <= 0)
        {
            return Result.Failure<ProductVariant>(
                BasketErrors.OutOfStock);
        }

        if (desiredQuantity > variant.StockCount)
        {
            return Result.Failure<ProductVariant>(
                BasketErrors.InsufficientStock(
                    variant.StockCount));
        }

        return Result.Success(variant);
    }

    private static Error ValidateQuantity(
        int quantity)
    {
        if (quantity <= 0)
        {
            return BasketErrors.InvalidQuantity(
                "Miqdar ən azı 1 olmalıdır.");
        }

        if (quantity > BasketItem.MaximumQuantity)
        {
            return BasketErrors.InvalidQuantity(
                $"Bir məhsuldan maksimum " +
                $"{BasketItem.MaximumQuantity} ədəd " +
                "səbətə əlavə etmək olar.");
        }

        return Error.None;
    }
}
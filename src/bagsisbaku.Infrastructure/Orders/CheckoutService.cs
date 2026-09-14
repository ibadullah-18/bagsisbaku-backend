using System.Data;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Orders;
using bagsisbaku.Domain.Baskets;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Customers;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Orders;

internal sealed class CheckoutService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IClock clock,
    IValidator<PlaceOrderCommand> validator,
    IOrderNumberGenerator orderNumberGenerator,
    IDeliveryFeeCalculator deliveryFeeCalculator)
    : ICheckoutService
{
    public async Task<Result<PlacedOrderModel>>
        PlaceOrderAsync(
            PlaceOrderCommand command,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult =
            await validator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var description =
                string.Join(
                    " ",
                    validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .Distinct(
                            StringComparer.Ordinal));

            return Result.Failure<PlacedOrderModel>(
                Error.Validation(
                    "checkout.validation",
                    description));
        }

        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            return Result.Failure<PlacedOrderModel>(
                CheckoutErrors.AuthenticationRequired);
        }

        var selectedLanguage =
            Enum.IsDefined(language)
                ? language
                : SupportedLanguage.Azerbaijani;

        var executionStrategy =
            dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                dbContext.ChangeTracker.Clear();

                return await PlaceOrderCoreAsync(
                    userId,
                    command,
                    selectedLanguage,
                    cancellationToken);
            });
    }

    private async Task<Result<PlacedOrderModel>>
        PlaceOrderCoreAsync(
            Guid userId,
            PlaceOrderCommand command,
            SupportedLanguage language,
            CancellationToken cancellationToken)
    {
        await using var transaction =
            await dbContext.Database
                .BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

        try
        {
            var user =
                await dbContext.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        item => item.Id == userId,
                        cancellationToken);

            if (user is null)
            {
                return Result.Failure<PlacedOrderModel>(
                    CheckoutErrors.CustomerNotFound);
            }

            if (!user.IsActive)
            {
                return Result.Failure<PlacedOrderModel>(
                    CheckoutErrors.CustomerInactive);
            }

            var basket =
                await dbContext.Baskets
                    .Include(item => item.Items)
                    .SingleOrDefaultAsync(
                        item => item.UserId == userId,
                        cancellationToken);

            if (basket is null ||
                basket.Items.Count == 0)
            {
                return Result.Failure<PlacedOrderModel>(
                    CheckoutErrors.BasketEmpty);
            }

            var deliveryResult =
                await CreateDeliverySnapshotAsync(
                    userId,
                    command,
                    cancellationToken);

            if (deliveryResult.IsFailure)
            {
                return Result.Failure<PlacedOrderModel>(
                    deliveryResult.Error);
            }

            var delivery =
                deliveryResult.Value;

            var orderNumber =
                await CreateUniqueOrderNumberAsync(
                    cancellationToken);

            if (orderNumber is null)
            {
                return Result.Failure<PlacedOrderModel>(
                    CheckoutErrors.OrderNumberConflict);
            }

            var basketItems =
                basket.Items.ToArray();

            var variantIds =
                basketItems
                    .Select(
                        item =>
                            item.ProductVariantId)
                    .Distinct()
                    .ToArray();

            var variants =
                await dbContext.ProductVariants
                    .Where(
                        variant =>
                            variantIds.Contains(
                                variant.Id))
                    .ToListAsync(
                        cancellationToken);

            if (variants.Count != variantIds.Length)
            {
                return Result.Failure<PlacedOrderModel>(
                    CheckoutErrors.ProductVariantUnavailable);
            }

            var productIds =
                variants
                    .Select(
                        variant =>
                            variant.ProductId)
                    .Distinct()
                    .ToArray();

            var sizeIds =
                variants
                    .Select(
                        variant =>
                            variant.SizeId)
                    .Distinct()
                    .ToArray();

            var colorIds =
                variants
                    .Select(
                        variant =>
                            variant.ColorId)
                    .Distinct()
                    .ToArray();

            var products =
                await dbContext.Products
                    .AsNoTracking()
                    .Where(
                        product =>
                            productIds.Contains(
                                product.Id))
                    .ToListAsync(
                        cancellationToken);

            if (products.Count != productIds.Length)
            {
                return Result.Failure<PlacedOrderModel>(
                    CheckoutErrors.ProductUnavailable);
            }

            var brandIds =
                products
                    .Select(product => product.BrandId)
                    .Distinct()
                    .ToArray();

            var brands =
                await dbContext.Brands
                    .AsNoTracking()
                    .Where(
                        brand =>
                            brandIds.Contains(brand.Id))
                    .ToListAsync(
                        cancellationToken);

            var sizes =
                await dbContext.Sizes
                    .AsNoTracking()
                    .Where(
                        size =>
                            sizeIds.Contains(size.Id))
                    .ToListAsync(
                        cancellationToken);

            var colors =
                await dbContext.Colors
                    .AsNoTracking()
                    .Where(
                        color =>
                            colorIds.Contains(color.Id))
                    .ToListAsync(
                        cancellationToken);

            var images =
                await dbContext.ProductImages
                    .AsNoTracking()
                    .Where(
                        image =>
                            productIds.Contains(
                                image.ProductId))
                    .ToListAsync(
                        cancellationToken);

            var productTranslations =
                await dbContext.ProductTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            productIds.Contains(
                                translation.ProductId) &&
                            translation.Language ==
                            language)
                    .ToListAsync(
                        cancellationToken);

            var sizeTranslations =
                await dbContext.SizeTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            sizeIds.Contains(
                                translation.SizeId) &&
                            translation.Language ==
                            language)
                    .ToListAsync(
                        cancellationToken);

            var colorTranslations =
                await dbContext.ColorTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            colorIds.Contains(
                                translation.ColorId) &&
                            translation.Language ==
                            language)
                    .ToListAsync(
                        cancellationToken);

            var variantById =
                variants.ToDictionary(
                    variant => variant.Id);

            var productById =
                products.ToDictionary(
                    product => product.Id);

            var brandById =
                brands.ToDictionary(
                    brand => brand.Id);

            var sizeById =
                sizes.ToDictionary(
                    size => size.Id);

            var colorById =
                colors.ToDictionary(
                    color => color.Id);

            var productNameById =
                productTranslations.ToDictionary(
                    translation =>
                        translation.ProductId,
                    translation =>
                        translation.Name);

            var sizeValueById =
                sizeTranslations.ToDictionary(
                    translation =>
                        translation.SizeId,
                    translation =>
                        translation.Value);

            var colorNameById =
                colorTranslations.ToDictionary(
                    translation =>
                        translation.ColorId,
                    translation =>
                        translation.Name);

            var imageByProductId =
                images
                    .GroupBy(
                        image => image.ProductId)
                    .ToDictionary(
                        group => group.Key,
                        group =>
                            group
                                .OrderByDescending(
                                    image =>
                                        image.IsPrimary)
                                .ThenBy(
                                    image =>
                                        image.SortOrder)
                                .Select(
                                    image =>
                                        image.ImageUrl)
                                .FirstOrDefault());

            var snapshots =
                new List<OrderItemSnapshot>(
                    basketItems.Length);

            foreach (var basketItem in basketItems)
            {
                if (!variantById.TryGetValue(
                        basketItem.ProductVariantId,
                        out var variant))
                {
                    return Result.Failure<PlacedOrderModel>(
                        CheckoutErrors
                            .ProductVariantUnavailable);
                }

                if (!productById.TryGetValue(
                        variant.ProductId,
                        out var product) ||
                    !product.IsActive)
                {
                    return Result.Failure<PlacedOrderModel>(
                        CheckoutErrors.ProductUnavailable);
                }

                if (!brandById.TryGetValue(
                        product.BrandId,
                        out var brand) ||
                    !brand.IsActive)
                {
                    return Result.Failure<PlacedOrderModel>(
                        CheckoutErrors.ProductUnavailable);
                }

                if (!variant.IsActive ||
                    !sizeById.TryGetValue(
                        variant.SizeId,
                        out var size) ||
                    !size.IsActive ||
                    !colorById.TryGetValue(
                        variant.ColorId,
                        out var color) ||
                    !color.IsActive)
                {
                    return Result.Failure<PlacedOrderModel>(
                        CheckoutErrors
                            .ProductVariantUnavailable);
                }

                if (variant.StockCount <= 0)
                {
                    return Result.Failure<PlacedOrderModel>(
                        CheckoutErrors.OutOfStock);
                }

                if (basketItem.Quantity >
                    variant.StockCount)
                {
                    return Result.Failure<PlacedOrderModel>(
                        CheckoutErrors.InsufficientStock(
                            product.ProductCode,
                            variant.StockCount));
                }

                var unitPrice =
                    product.DiscountPrice ??
                    product.Price;

                var productName =
                    productNameById.GetValueOrDefault(
                        product.Id,
                        product.Name);

                var sizeValue =
                    sizeValueById.GetValueOrDefault(
                        size.Id,
                        size.Value);

                var colorName =
                    colorNameById.GetValueOrDefault(
                        color.Id,
                        color.Name);

                imageByProductId.TryGetValue(
                    product.Id,
                    out var imageUrl);

                snapshots.Add(
                    new OrderItemSnapshot(
                        product.Id,
                        variant.Id,
                        productName,
                        product.ProductCode,
                        product.ProductType,
                        brand.Name,
                        imageUrl,
                        size.Id,
                        sizeValue,
                        color.Id,
                        colorName,
                        color.HexCode,
                        product.Price,
                        unitPrice,
                        basketItem.Quantity));

                variant.DecreaseStock(
                    basketItem.Quantity);
            }

            var deliveryFee =
                deliveryFeeCalculator.Calculate(
                    command.DeliveryType,
                    delivery.Latitude,
                    delivery.Longitude);

            var utcNow = clock.UtcNow;

            var order =
                Order.Create(
                    userId,
                    orderNumber,
                    command.DeliveryType,
                    PaymentMethod.Cash,
                    delivery,
                    command.CustomerNote,
                    deliveryFee,
                    utcNow,
                    snapshots);

            dbContext.Orders.Add(order);

            basket.Clear();

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result.Success(
                CheckoutModelMapper.ToModel(
                    order,
                    language));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<PlacedOrderModel>(
                CheckoutErrors.ConcurrencyConflict);
        }
        catch (DomainException)
        {
            return Result.Failure<PlacedOrderModel>(
                CheckoutErrors.ConcurrencyConflict);
        }
    }

    private async Task<Result<OrderDeliverySnapshot>>
        CreateDeliverySnapshotAsync(
            Guid userId,
            PlaceOrderCommand command,
            CancellationToken cancellationToken)
    {
        if (command.DeliveryType ==
            DeliveryType.StorePickup)
        {
            var delivery =
                OrderDeliverySnapshot.ForStorePickup(
                    command.PickupRecipientFullName!,
                    command.PickupPhoneNumber!);

            return Result.Success(delivery);
        }

        if (command.CustomerAddressId is not Guid addressId ||
            addressId == Guid.Empty)
        {
            return Result.Failure<OrderDeliverySnapshot>(
                CheckoutErrors.AddressRequired);
        }

        var address =
            await dbContext.CustomerAddresses
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == addressId &&
                        item.UserId == userId,
                    cancellationToken);

        if (address is null)
        {
            return Result.Failure<OrderDeliverySnapshot>(
                CheckoutErrors.AddressNotFound);
        }

        var snapshot =
            OrderDeliverySnapshot.ForAddressDelivery(
                address.Id,
                address.RecipientFullName,
                address.PhoneNumber,
                address.City,
                address.District,
                address.AddressLine,
                address.PostalCode,
                address.DeliveryNote,
                address.Latitude,
                address.Longitude);

        return Result.Success(snapshot);
    }

    private async Task<string?>
        CreateUniqueOrderNumberAsync(
            CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var orderNumber =
                orderNumberGenerator.Generate(
                    clock.UtcNow);

            var alreadyExists =
                await dbContext.Orders
                    .AsNoTracking()
                    .AnyAsync(
                        order =>
                            order.OrderNumber ==
                            orderNumber,
                        cancellationToken);

            if (!alreadyExists)
            {
                return orderNumber;
            }
        }

        return null;
    }
}
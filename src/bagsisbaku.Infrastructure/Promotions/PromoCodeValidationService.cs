using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Baskets;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Promotions.Customer;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Domain.Promotions;
using bagsisbaku.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Promotions;

internal sealed class PromoCodeValidationService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IClock clock,
    IBasketService basketService,
    IValidator<ValidatePromoCodeCommand> validator)
    : IPromoCodeValidationService
{
    public async Task<Result<PromoCodePreviewModel>>
        ValidateAsync(
            ValidatePromoCodeCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        var validationResult =
            await validator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    ToValidationError(
                        validationResult));
        }

        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors
                        .AuthenticationRequired);
        }

        var basketResult =
            await basketService.GetBasketAsync(
                SupportedLanguage.Azerbaijani,
                cancellationToken);

        if (basketResult.IsFailure)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    basketResult.Error);
        }

        var basket =
            basketResult.Value;

        if (basket.Items.Count == 0 ||
            basket.Total <= 0)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors.BasketEmpty);
        }

        if (!basket.CanCheckout)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors
                        .BasketUnavailable);
        }

        var normalizedCode =
            command.Code
                .Trim()
                .ToUpperInvariant();

        var promoCode =
            await dbContext.PromoCodes
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    entity =>
                        entity.Code ==
                        normalizedCode,
                    cancellationToken);

        if (promoCode is null)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors.InvalidCode);
        }

        var utcNow =
            clock.UtcNow;

        if (!promoCode.IsActive)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors.Inactive);
        }

        if (utcNow < promoCode.StartsAtUtc)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors.NotStarted);
        }

        if (utcNow >= promoCode.EndsAtUtc)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors.Expired);
        }

        if (promoCode.UsageLimit.HasValue &&
            promoCode.UsageCount >=
            promoCode.UsageLimit.Value)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors
                        .UsageLimitReached);
        }

        if (promoCode.PerCustomerUsageLimit
            .HasValue)
        {
            var customerUsageCount =
                await dbContext.PromoCodeUsages
                    .AsNoTracking()
                    .CountAsync(
                        usage =>
                            usage.PromoCodeId ==
                                promoCode.Id &&
                            usage.UserId ==
                                userId &&
                            usage.ReleasedAtUtc ==
                                null,
                        cancellationToken);

            if (customerUsageCount >=
                promoCode.PerCustomerUsageLimit
                    .Value)
            {
                return Result.Failure<
                    PromoCodePreviewModel>(
                        PromoCodeErrors
                            .CustomerUsageLimitReached);
            }
        }

        if (!promoCode.MeetsMinimumOrder(
            basket.Total))
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors
                        .MinimumOrderNotMet(
                            promoCode
                                .MinimumOrderAmount));
        }

        decimal discountAmount;

        try
        {
            discountAmount =
                promoCode.CalculateDiscount(
                    basket.Total);
        }
        catch (DomainException exception)
        {
            return Result.Failure<
                PromoCodePreviewModel>(
                    PromoCodeErrors.Validation(
                        exception.Message));
        }

        var totalAfterDiscount =
            decimal.Round(
                Math.Max(
                    basket.Total -
                    discountAmount,
                    0m),
                2,
                MidpointRounding.AwayFromZero);

        return Result.Success(
            new PromoCodePreviewModel(
                promoCode.Id,
                promoCode.Code,
                (int)promoCode.DiscountType,
                ToDiscountTypeName(
                    promoCode.DiscountType),
                promoCode.DiscountValue,
                promoCode.MinimumOrderAmount,
                promoCode.MaximumDiscountAmount,
                basket.Total,
                discountAmount,
                totalAfterDiscount,
                promoCode.StartsAtUtc,
                promoCode.EndsAtUtc));
    }

    private static string ToDiscountTypeName(
        PromotionDiscountType discountType)
    {
        return discountType switch
        {
            PromotionDiscountType.Percentage =>
                "Faiz",

            PromotionDiscountType.FixedAmount =>
                "Sabit məbləğ",

            _ =>
                "Naməlum"
        };
    }

    private static Error ToValidationError(
        ValidationResult validationResult)
    {
        var description =
            string.Join(
                " ",
                validationResult.Errors
                    .Select(error =>
                        error.ErrorMessage)
                    .Distinct(
                        StringComparer.Ordinal));

        return PromoCodeErrors.Validation(
            description);
    }
}
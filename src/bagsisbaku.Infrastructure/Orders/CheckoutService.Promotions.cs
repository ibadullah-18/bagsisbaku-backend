using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Promotions;
using bagsisbaku.Application.Promotions.Customer;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Domain.Promotions;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Orders;

internal sealed partial class CheckoutService
{
    private static async Task<
        Result<ValidatedPromotion?>>
        ValidatePromotionAsync(
            ApplicationDbContext dbContext,
            Guid userId,
            string? requestedCode,
            decimal orderAmount,
            DateTimeOffset utcNow,
            CancellationToken cancellationToken)
    {
        var normalizedCode =
            PromoCodeNormalizer.Normalize(
                requestedCode);

        if (normalizedCode is null)
        {
            return Result.Success<
                ValidatedPromotion?>(null);
        }

        var promoCode =
            await dbContext.PromoCodes
                .SingleOrDefaultAsync(
                    entity =>
                        entity.Code ==
                        normalizedCode,
                    cancellationToken);

        if (promoCode is null)
        {
            return Result.Failure<
                ValidatedPromotion?>(
                    PromoCodeErrors.InvalidCode);
        }

        if (!promoCode.IsActive)
        {
            return Result.Failure<
                ValidatedPromotion?>(
                    PromoCodeErrors.Inactive);
        }

        if (utcNow < promoCode.StartsAtUtc)
        {
            return Result.Failure<
                ValidatedPromotion?>(
                    PromoCodeErrors.NotStarted);
        }

        if (utcNow >= promoCode.EndsAtUtc)
        {
            return Result.Failure<
                ValidatedPromotion?>(
                    PromoCodeErrors.Expired);
        }

        if (promoCode.UsageLimit.HasValue &&
            promoCode.UsageCount >=
            promoCode.UsageLimit.Value)
        {
            return Result.Failure<
                ValidatedPromotion?>(
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
                    ValidatedPromotion?>(
                        PromoCodeErrors
                            .CustomerUsageLimitReached);
            }
        }

        if (!promoCode.MeetsMinimumOrder(
            orderAmount))
        {
            return Result.Failure<
                ValidatedPromotion?>(
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
                    orderAmount);
        }
        catch (DomainException)
        {
            return Result.Failure<
                ValidatedPromotion?>(
                    PromoCodeErrors
                        .UnexpectedFailure);
        }

        if (discountAmount <= 0)
        {
            return Result.Failure<
                ValidatedPromotion?>(
                    PromoCodeErrors
                        .UnexpectedFailure);
        }

        var snapshot =
            new OrderPromotionSnapshot(
                promoCode.Id,
                promoCode.Code,
                discountAmount);

        return Result.Success<
            ValidatedPromotion?>(
                new ValidatedPromotion(
                    promoCode,
                    snapshot,
                    discountAmount));
    }

    private static void RegisterPromotionUsage(
        ApplicationDbContext dbContext,
        ValidatedPromotion promotion,
        Guid userId,
        Guid orderId,
        DateTimeOffset usedAtUtc)
    {
        promotion.PromoCode.RegisterUsage(
            usedAtUtc);

        var usage =
            PromoCodeUsage.Create(
                promotion.PromoCode.Id,
                userId,
                orderId,
                promotion.DiscountAmount,
                usedAtUtc);

        dbContext.PromoCodeUsages.Add(
            usage);
    }

    private sealed record ValidatedPromotion(
        PromoCode PromoCode,
        OrderPromotionSnapshot Snapshot,
        decimal DiscountAmount);
}
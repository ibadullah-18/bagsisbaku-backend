using bagsisbaku.Domain.Promotions;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.IntegrationTests;

public sealed partial class CatalogApiFixture
{
    public async Task<Guid> CreatePromoCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (_factory is null)
        {
            throw new InvalidOperationException(
                "Integration test factory başladılmayıb.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            code);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<
                ApplicationDbContext>();

        var utcNow =
            DateTimeOffset.UtcNow;

        var promoCode =
            PromoCode.Create(
                name:
                    "Integration test promo kodu",
                code:
                    code,
                discountType:
                    PromotionDiscountType.FixedAmount,
                discountValue:
                    25m,
                minimumOrderAmount:
                    100m,
                maximumDiscountAmount:
                    null,
                usageLimit:
                    100,
                perCustomerUsageLimit:
                    1,
                startsAtUtc:
                    utcNow.AddDays(-1),
                endsAtUtc:
                    utcNow.AddDays(1));

        dbContext.PromoCodes.Add(
            promoCode);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return promoCode.Id;
    }

    public async Task<PromoReleaseState>
        GetPromoReleaseStateAsync(
            Guid promoCodeId,
            Guid orderId,
            CancellationToken cancellationToken = default)
    {
        if (_factory is null)
        {
            throw new InvalidOperationException(
                "Integration test factory başladılmayıb.");
        }

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<
                ApplicationDbContext>();

        var usageCount =
            await dbContext.PromoCodes
                .AsNoTracking()
                .Where(
                    promoCode =>
                        promoCode.Id ==
                        promoCodeId)
                .Select(
                    promoCode =>
                        promoCode.UsageCount)
                .SingleAsync(
                    cancellationToken);

        var usage =
            await dbContext.PromoCodeUsages
                .AsNoTracking()
                .Where(
                    item =>
                        item.PromoCodeId ==
                            promoCodeId &&
                        item.OrderId ==
                            orderId)
                .Select(
                    item =>
                        new
                        {
                            item.Id,
                            item.ReleasedAtUtc
                        })
                .SingleOrDefaultAsync(
                    cancellationToken);

        return new PromoReleaseState(
            usageCount,
            usage is not null,
            usage?.ReleasedAtUtc);
    }
}

public sealed record PromoReleaseState(
    int UsageCount,
    bool UsageExists,
    DateTimeOffset? ReleasedAtUtc);
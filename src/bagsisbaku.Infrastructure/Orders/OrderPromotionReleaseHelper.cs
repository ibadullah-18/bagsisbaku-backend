using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Orders;

internal static class OrderPromotionReleaseHelper
{
    public static async Task ReleaseAsync(
        ApplicationDbContext dbContext,
        Order order,
        DateTimeOffset releasedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            dbContext);

        ArgumentNullException.ThrowIfNull(
            order);

        if (!order.PromoCodeId.HasValue)
        {
            return;
        }

        var usage =
            await dbContext.PromoCodeUsages
                .SingleOrDefaultAsync(
                    item =>
                        item.OrderId == order.Id &&
                        item.PromoCodeId ==
                        order.PromoCodeId.Value,
                    cancellationToken);

        if (usage is null ||
            usage.ReleasedAtUtc.HasValue)
        {
            return;
        }

        var promoCode =
            await dbContext.PromoCodes
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        usage.PromoCodeId,
                    cancellationToken);

        if (promoCode is null)
        {
            throw new DomainException(
                "Sifarişə aid promo kod tapılmadı.");
        }

        promoCode.ReleaseUsage();

        usage.Release(
            releasedAtUtc);
    }
}
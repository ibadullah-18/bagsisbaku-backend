using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Promotions;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class PromoCodeTests
{
    private static readonly DateTimeOffset StartsAtUtc =
        new(
            2026,
            9,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

    private static readonly DateTimeOffset EndsAtUtc =
        StartsAtUtc.AddDays(30);

    [Fact]
    public void CreateShouldNormalizeCode()
    {
        var promoCode =
            CreatePromoCode(
                code: "  bagsis-20  ");

        Assert.Equal(
            "BAGSIS-20",
            promoCode.Code);

        Assert.Equal(
            "Sentyabr endirimi",
            promoCode.Name);

        Assert.True(
            promoCode.IsActive);

        Assert.Equal(
            0,
            promoCode.UsageCount);
    }

    [Fact]
    public void PercentageDiscountShouldRespectMaximum()
    {
        var promoCode =
            CreatePromoCode(
                discountType:
                    PromotionDiscountType.Percentage,
                discountValue: 20m,
                maximumDiscountAmount: 50m);

        var discount =
            promoCode.CalculateDiscount(
                300m);

        Assert.Equal(
            50m,
            discount);
    }

    [Fact]
    public void FixedDiscountShouldNotExceedOrderAmount()
    {
        var promoCode =
            CreatePromoCode(
                discountType:
                    PromotionDiscountType.FixedAmount,
                discountValue: 80m,
                minimumOrderAmount: 0m,
                maximumDiscountAmount: null);

        var discount =
            promoCode.CalculateDiscount(
                50m);

        Assert.Equal(
            50m,
            discount);
    }

    [Fact]
    public void CreateShouldRejectPercentageAboveOneHundred()
    {
        Assert.Throws<DomainException>(
            () =>
                CreatePromoCode(
                    discountType:
                        PromotionDiscountType.Percentage,
                    discountValue: 101m));
    }

    [Fact]
    public void CalculateDiscountShouldRejectSmallOrder()
    {
        var promoCode =
            CreatePromoCode(
                minimumOrderAmount: 100m);

        Assert.Throws<DomainException>(
            () =>
                promoCode.CalculateDiscount(
                    99.99m));
    }

    [Fact]
    public void AvailabilityShouldRespectScheduleAndStatus()
    {
        var promoCode =
            CreatePromoCode();

        Assert.False(
            promoCode.IsAvailableAt(
                StartsAtUtc.AddSeconds(-1)));

        Assert.True(
            promoCode.IsAvailableAt(
                StartsAtUtc));

        Assert.False(
            promoCode.IsAvailableAt(
                EndsAtUtc));

        promoCode.Deactivate();

        Assert.False(
            promoCode.IsAvailableAt(
                StartsAtUtc.AddDays(1)));
    }

    [Fact]
    public void UsageLimitShouldPreventAdditionalUsage()
    {
        var promoCode =
            CreatePromoCode(
                usageLimit: 1);

        promoCode.RegisterUsage(
            StartsAtUtc.AddDays(1));

        Assert.Equal(
            1,
            promoCode.UsageCount);

        Assert.False(
            promoCode.IsAvailableAt(
                StartsAtUtc.AddDays(2)));

        Assert.Throws<DomainException>(
            () =>
                promoCode.RegisterUsage(
                    StartsAtUtc.AddDays(2)));
    }

    [Fact]
    public void PromoCodeUsageShouldStoreSnapshot()
    {
        var promoCodeId =
            Guid.NewGuid();

        var userId =
            Guid.NewGuid();

        var orderId =
            Guid.NewGuid();

        var usedAtUtc =
            StartsAtUtc.AddDays(2);

        var usage =
            PromoCodeUsage.Create(
                promoCodeId,
                userId,
                orderId,
                25m,
                usedAtUtc);

        Assert.NotEqual(
            Guid.Empty,
            usage.Id);

        Assert.Equal(
            promoCodeId,
            usage.PromoCodeId);

        Assert.Equal(
            userId,
            usage.UserId);

        Assert.Equal(
            orderId,
            usage.OrderId);

        Assert.Equal(
            25m,
            usage.DiscountAmount);

        Assert.Equal(
            usedAtUtc,
            usage.UsedAtUtc);
    }

    private static PromoCode CreatePromoCode(
        string name = "Sentyabr endirimi",
        string code = "BAGSIS20",
        PromotionDiscountType discountType =
            PromotionDiscountType.Percentage,
        decimal discountValue = 20m,
        decimal minimumOrderAmount = 50m,
        decimal? maximumDiscountAmount = 100m,
        int? usageLimit = 100,
        int? perCustomerUsageLimit = 1)
    {
        return PromoCode.Create(
            name,
            code,
            discountType,
            discountValue,
            minimumOrderAmount,
            maximumDiscountAmount,
            usageLimit,
            perCustomerUsageLimit,
            StartsAtUtc,
            EndsAtUtc);
    }
}
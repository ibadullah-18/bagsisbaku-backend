using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Promotions;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class PromoCodeReleaseTests
{
    [Fact]
    public void ReleaseUsageShouldDecreaseUsageCount()
    {
        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                18,
                10,
                0,
                0,
                TimeSpan.Zero);

        var promoCode =
            CreatePromoCode(
                utcNow);

        promoCode.RegisterUsage(
            utcNow);

        Assert.Equal(
            1,
            promoCode.UsageCount);

        promoCode.ReleaseUsage();

        Assert.Equal(
            0,
            promoCode.UsageCount);
    }

    [Fact]
    public void ReleaseUsageShouldRejectZeroUsageCount()
    {
        var utcNow =
            new DateTimeOffset(
                2026,
                9,
                18,
                10,
                0,
                0,
                TimeSpan.Zero);

        var promoCode =
            CreatePromoCode(
                utcNow);

        var exception =
            Assert.Throws<DomainException>(
                promoCode.ReleaseUsage);

        Assert.Contains(
            "sıfırdan aşağı",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReleaseShouldStoreReleasedAtUtc()
    {
        var usedAtUtc =
            new DateTimeOffset(
                2026,
                9,
                18,
                10,
                0,
                0,
                TimeSpan.Zero);

        var releasedAtUtc =
            usedAtUtc.AddMinutes(20);

        var usage =
            CreateUsage(
                usedAtUtc);

        usage.Release(
            releasedAtUtc);

        Assert.True(
            usage.IsReleased);

        Assert.Equal(
            releasedAtUtc,
            usage.ReleasedAtUtc);
    }

    [Fact]
    public void ReleaseShouldBeIdempotent()
    {
        var usedAtUtc =
            new DateTimeOffset(
                2026,
                9,
                18,
                10,
                0,
                0,
                TimeSpan.Zero);

        var firstReleasedAtUtc =
            usedAtUtc.AddMinutes(20);

        var secondReleasedAtUtc =
            usedAtUtc.AddMinutes(40);

        var usage =
            CreateUsage(
                usedAtUtc);

        usage.Release(
            firstReleasedAtUtc);

        usage.Release(
            secondReleasedAtUtc);

        Assert.Equal(
            firstReleasedAtUtc,
            usage.ReleasedAtUtc);
    }

    [Fact]
    public void ReleaseShouldRejectTimeBeforeUsage()
    {
        var usedAtUtc =
            new DateTimeOffset(
                2026,
                9,
                18,
                10,
                0,
                0,
                TimeSpan.Zero);

        var usage =
            CreateUsage(
                usedAtUtc);

        var exception =
            Assert.Throws<DomainException>(
                () =>
                    usage.Release(
                        usedAtUtc.AddMinutes(-1)));

        Assert.Contains(
            "istifadə vaxtından əvvəl",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private static PromoCode CreatePromoCode(
        DateTimeOffset utcNow)
    {
        return PromoCode.Create(
            "Sınaq promo kodu",
            "RELEASE20",
            PromotionDiscountType.Percentage,
            20m,
            50m,
            100m,
            10,
            1,
            utcNow.AddDays(-1),
            utcNow.AddDays(1));
    }

    private static PromoCodeUsage CreateUsage(
        DateTimeOffset usedAtUtc)
    {
        return PromoCodeUsage.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            20m,
            usedAtUtc);
    }
}
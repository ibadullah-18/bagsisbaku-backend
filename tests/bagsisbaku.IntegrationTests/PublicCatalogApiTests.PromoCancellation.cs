using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task CustomerCancellationReleasesPromoUsage()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        await EnsureBasketIsEmptyAsync(
            cancellationToken);

        var promoCode =
            $"CUSTOMER-{Guid.NewGuid():N}"[..17]
                .ToUpperInvariant();

        var promoCodeId =
            await _fixture.CreatePromoCodeAsync(
                promoCode,
                cancellationToken);

        var stockBeforeCheckout =
            await GetShoeStockAsync(
                cancellationToken);

        using var addResponse =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    "/api/basket/items?lang=az",
                    new
                    {
                        productVariantId =
                            _fixture.ShoeVariantId,

                        quantity = 1
                    },
                    cancellationToken);

        addResponse.EnsureSuccessStatusCode();

        using var checkoutResponse =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    "/api/orders?lang=az",
                    new
                    {
                        deliveryType = 1,

                        customerAddressId =
                            (Guid?)null,

                        pickupRecipientFullName =
                            "Promo Test Customer",

                        pickupPhoneNumber =
                            "+994501234567",

                        customerNote =
                            "Promo release integration testi.",

                        promoCode
                    },
                    cancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            checkoutResponse.StatusCode);

        var placedOrder =
            await checkoutResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var orderId =
            placedOrder
                .GetProperty("id")
                .GetGuid();

        Assert.NotEqual(
            Guid.Empty,
            orderId);

        Assert.Equal(
            promoCode,
            placedOrder
                .GetProperty("promoCode")
                .GetString());

        Assert.Equal(
            25m,
            placedOrder
                .GetProperty("promoDiscountAmount")
                .GetDecimal());

        Assert.Equal(
            65.10m,
            placedOrder
                .GetProperty("totalDiscountAmount")
                .GetDecimal());

        Assert.Equal(
            154.90m,
            placedOrder
                .GetProperty("total")
                .GetDecimal());

        var stockAfterCheckout =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.Equal(
            stockBeforeCheckout - 1,
            stockAfterCheckout);

        var stateAfterCheckout =
            await _fixture.GetPromoReleaseStateAsync(
                promoCodeId,
                orderId,
                cancellationToken);

        Assert.Equal(
            1,
            stateAfterCheckout.UsageCount);

        Assert.True(
            stateAfterCheckout.UsageExists);

        Assert.Null(
            stateAfterCheckout.ReleasedAtUtc);

        using var cancelResponse =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    $"/api/orders/{orderId}/cancel?lang=az",
                    new
                    {
                        reason =
                            "Promo release testi üçün ləğv edildi."
                    },
                    cancellationToken);

        cancelResponse.EnsureSuccessStatusCode();

        var cancelledOrder =
            await cancelResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            5,
            cancelledOrder
                .GetProperty("status")
                .GetInt32());

        Assert.False(
            cancelledOrder
                .GetProperty("canBeCancelled")
                .GetBoolean());

        Assert.Equal(
            promoCode,
            cancelledOrder
                .GetProperty("promoCode")
                .GetString());

        var stateAfterCancellation =
            await _fixture.GetPromoReleaseStateAsync(
                promoCodeId,
                orderId,
                cancellationToken);

        Assert.Equal(
            0,
            stateAfterCancellation.UsageCount);

        Assert.True(
            stateAfterCancellation.UsageExists);

        Assert.NotNull(
            stateAfterCancellation.ReleasedAtUtc);

        var stockAfterCancellation =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.Equal(
            stockBeforeCheckout,
            stockAfterCancellation);
    }
}
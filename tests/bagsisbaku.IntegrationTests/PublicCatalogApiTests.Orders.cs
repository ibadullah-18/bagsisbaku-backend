using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task CustomerOrderEndpointsRequireAuthentication()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var listResponse =
            await _fixture.Client.GetAsync(
                "/api/orders?page=1&pageSize=10",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            listResponse.StatusCode);

        var orderId =
            Guid.NewGuid();

        using var detailsResponse =
            await _fixture.Client.GetAsync(
                $"/api/orders/{orderId}?lang=az",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            detailsResponse.StatusCode);

        using var cancelResponse =
            await _fixture.Client.PostAsJsonAsync(
                $"/api/orders/{orderId}/cancel?lang=az",
                new
                {
                    reason =
                        "Authentication integration testi."
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            cancelResponse.StatusCode);
    }

    [Fact]
    public async Task InvalidOrderPaginationReturnsBadRequest()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var response =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/orders?page=0&pageSize=10",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            "orders.invalid-pagination",
            problem
                .GetProperty("code")
                .GetString());
    }

    [Fact]
    public async Task CustomerOrderLifecycleWorksAndRestoresStock()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        var stockBeforeCheckout =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.Equal(
            8,
            stockBeforeCheckout);

        using var addToBasketResponse =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    "/api/basket/items?lang=az",
                    new
                    {
                        productVariantId =
                            _fixture.ShoeVariantId,

                        quantity = 2
                    },
                    cancellationToken);

        addToBasketResponse.EnsureSuccessStatusCode();

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
                            "Order Test Customer",

                        pickupPhoneNumber =
                            "+994501234567",

                        customerNote =
                            "Integration test sifarişi."
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

        var orderNumber =
            placedOrder
                .GetProperty("orderNumber")
                .GetString();

        Assert.NotEqual(
            Guid.Empty,
            orderId);

        Assert.NotNull(
            orderNumber);

        Assert.StartsWith(
            "bagsisbaku-",
            orderNumber);

        Assert.Equal(
            "az",
            placedOrder
                .GetProperty("language")
                .GetString());

        Assert.Equal(
            1,
            placedOrder
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            1,
            placedOrder
                .GetProperty("deliveryType")
                .GetInt32());

        Assert.Equal(
            440m,
            placedOrder
                .GetProperty("subtotal")
                .GetDecimal());

        Assert.Equal(
            80.20m,
            placedOrder
                .GetProperty("discountAmount")
                .GetDecimal());

        Assert.Equal(
            359.80m,
            placedOrder
                .GetProperty("total")
                .GetDecimal());

        Assert.True(
            placedOrder
                .GetProperty("canBeCancelled")
                .GetBoolean());

        var placedItems =
            placedOrder
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();

        var placedItem =
            Assert.Single(
                placedItems);

        Assert.Equal(
            _fixture.ShoeProductId,
            placedItem
                .GetProperty("productId")
                .GetGuid());

        Assert.Equal(
            _fixture.ShoeVariantId,
            placedItem
                .GetProperty("productVariantId")
                .GetGuid());

        Assert.Equal(
            2,
            placedItem
                .GetProperty("quantity")
                .GetInt32());

        var stockAfterCheckout =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.Equal(
            stockBeforeCheckout - 2,
            stockAfterCheckout);

        using var basketResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/basket?lang=az",
                cancellationToken);

        basketResponse.EnsureSuccessStatusCode();

        var basket =
            await basketResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            0,
            basket
                .GetProperty("totalQuantity")
                .GetInt32());

        Assert.Empty(
            basket
                .GetProperty("items")
                .EnumerateArray());

        using var orderListResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/orders?page=1&pageSize=10",
                cancellationToken);

        orderListResponse.EnsureSuccessStatusCode();

        var orderPage =
            await orderListResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            1,
            orderPage
                .GetProperty("page")
                .GetInt32());

        Assert.True(
            orderPage
                .GetProperty("totalCount")
                .GetInt32() >= 1);

        var orderSummaries =
            orderPage
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();

        var orderSummary =
            Assert.Single(
                orderSummaries.Where(
                    item =>
                        item
                            .GetProperty("id")
                            .GetGuid() == orderId));

        Assert.Equal(
            orderNumber,
            orderSummary
                .GetProperty("orderNumber")
                .GetString());

        Assert.Equal(
            1,
            orderSummary
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            1,
            orderSummary
                .GetProperty("uniqueItemCount")
                .GetInt32());

        Assert.Equal(
            2,
            orderSummary
                .GetProperty("totalQuantity")
                .GetInt32());

        Assert.Equal(
            359.80m,
            orderSummary
                .GetProperty("total")
                .GetDecimal());

        Assert.True(
            orderSummary
                .GetProperty("canBeCancelled")
                .GetBoolean());

        using var detailsResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                $"/api/orders/{orderId}?lang=az",
                cancellationToken);

        detailsResponse.EnsureSuccessStatusCode();

        var orderDetails =
            await detailsResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            orderId,
            orderDetails
                .GetProperty("id")
                .GetGuid());

        Assert.Equal(
            orderNumber,
            orderDetails
                .GetProperty("orderNumber")
                .GetString());

        Assert.Equal(
            "az",
            orderDetails
                .GetProperty("language")
                .GetString());

        Assert.Equal(
            1,
            orderDetails
                .GetProperty("status")
                .GetInt32());

        Assert.True(
            orderDetails
                .GetProperty("canBeCancelled")
                .GetBoolean());

        var detailItems =
            orderDetails
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();

        var detailItem =
            Assert.Single(
                detailItems);

        Assert.Equal(
            "Sarı adidas idman ayaqqabısı",
            detailItem
                .GetProperty("productName")
                .GetString());

        Assert.Equal(
            2,
            detailItem
                .GetProperty("quantity")
                .GetInt32());

        var initialHistory =
            orderDetails
                .GetProperty("statusHistory")
                .EnumerateArray()
                .ToArray();

        Assert.NotEmpty(
            initialHistory);

        Assert.Equal(
            1,
            initialHistory[0]
                .GetProperty("newStatus")
                .GetInt32());

        const string cancellationReason =
            "Müştəri integration test zamanı imtina etdi.";

        using var cancelResponse =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    $"/api/orders/{orderId}/cancel?lang=az",
                    new
                    {
                        reason =
                            cancellationReason
                    },
                    cancellationToken);

        cancelResponse.EnsureSuccessStatusCode();

        var cancelledOrder =
            await cancelResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            orderId,
            cancelledOrder
                .GetProperty("id")
                .GetGuid());

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
            JsonValueKind.String,
            cancelledOrder
                .GetProperty("cancelledAtUtc")
                .ValueKind);

        var cancelledHistory =
            cancelledOrder
                .GetProperty("statusHistory")
                .EnumerateArray()
                .ToArray();

        Assert.True(
            cancelledHistory.Length >= 2);

        var cancellationHistory =
            cancelledHistory[^1];

        Assert.Equal(
            1,
            cancellationHistory
                .GetProperty("previousStatus")
                .GetInt32());

        Assert.Equal(
            5,
            cancellationHistory
                .GetProperty("newStatus")
                .GetInt32());

        Assert.Equal(
            cancellationReason,
            cancellationHistory
                .GetProperty("note")
                .GetString());

        var stockAfterCancellation =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.Equal(
            stockBeforeCheckout,
            stockAfterCancellation);

        using var repeatedCancelResponse =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    $"/api/orders/{orderId}/cancel?lang=az",
                    new
                    {
                        reason =
                            "İkinci ləğv cəhdi."
                    },
                    cancellationToken);

        Assert.Equal(
            HttpStatusCode.Conflict,
            repeatedCancelResponse.StatusCode);

        var repeatedCancelProblem =
            await repeatedCancelResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            "orders.cannot-cancel",
            repeatedCancelProblem
                .GetProperty("code")
                .GetString());
    }

    private async Task<int> GetShoeStockAsync(
        CancellationToken cancellationToken)
    {
        using var response =
            await _fixture.Client.GetAsync(
                $"/api/catalog/products/" +
                $"{_fixture.ShoeProductId}" +
                "?language=az",
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var product =
            await response.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var variant =
            product
                .GetProperty("variants")
                .EnumerateArray()
                .Single(
                    item =>
                        item
                            .GetProperty("id")
                            .GetGuid() ==
                        _fixture.ShoeVariantId);

        return variant
            .GetProperty("stockCount")
            .GetInt32();
    }
}
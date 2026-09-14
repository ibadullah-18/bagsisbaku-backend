using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task AdminOrderEndpointsEnforcePermissions()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var customerResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/admin/orders?page=1&pageSize=20",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            customerResponse.StatusCode);

        using var viewerResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/orders?page=1&pageSize=20",
                _fixture.OrderViewerAccessToken,
                body: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            viewerResponse.StatusCode);

        using var viewerStatusResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{Guid.NewGuid()}" +
                "/status?lang=az",
                _fixture.OrderViewerAccessToken,
                new
                {
                    status = 2,
                    note =
                        "Viewer status dəyişmə testi."
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            viewerStatusResponse.StatusCode);
    }

    [Fact]
    public async Task AdminCanManageOrderAndCancellationRestoresStock()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        await EnsureBasketIsEmptyAsync(
            cancellationToken);

        var stockBeforeCheckout =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.True(
            stockBeforeCheckout > 0);

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
                            "Admin Order Test Customer",

                        pickupPhoneNumber =
                            "+994501112233",

                        customerNote =
                            "Admin order integration testi."
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

        Assert.Equal(
            1,
            placedOrder
                .GetProperty("status")
                .GetInt32());

        var stockAfterCheckout =
            await GetShoeStockAsync(
                cancellationToken);

        Assert.Equal(
            stockBeforeCheckout - 1,
            stockAfterCheckout);

        var encodedOrderNumber =
            Uri.EscapeDataString(
                orderNumber!);

        using var listResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/orders" +
                "?status=1" +
                "&deliveryType=1" +
                $"&search={encodedOrderNumber}" +
                "&page=1" +
                "&pageSize=20",
                _fixture.OrderViewerAccessToken,
                body: null,
                cancellationToken);

        listResponse.EnsureSuccessStatusCode();

        var orderPage =
            await listResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var summaries =
            orderPage
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();

        var summary =
            Assert.Single(
                summaries.Where(
                    item =>
                        item
                            .GetProperty("id")
                            .GetGuid() == orderId));

        Assert.Equal(
            orderNumber,
            summary
                .GetProperty("orderNumber")
                .GetString());

        Assert.Equal(
            1,
            summary
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            "Admin Order Test Customer",
            summary
                .GetProperty("recipientFullName")
                .GetString());

        using var detailsResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                $"/api/admin/orders/{orderId}?lang=az",
                _fixture.OrderViewerAccessToken,
                body: null,
                cancellationToken);

        detailsResponse.EnsureSuccessStatusCode();

        var pendingDetails =
            await detailsResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            orderId,
            pendingDetails
                .GetProperty("id")
                .GetGuid());

        Assert.Equal(
            1,
            pendingDetails
                .GetProperty("status")
                .GetInt32());

        Assert.True(
            pendingDetails
                .GetProperty("canBeCancelled")
                .GetBoolean());

        using var viewerUpdateResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{orderId}" +
                "/status?lang=az",
                _fixture.OrderViewerAccessToken,
                new
                {
                    status = 2,
                    note =
                        "Viewer status dəyişə bilməməlidir."
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            viewerUpdateResponse.StatusCode);

        using var invalidTransitionResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{orderId}" +
                "/status?lang=az",
                _fixture.OrderManagerAccessToken,
                new
                {
                    status = 4,
                    note =
                        "Pending sifariş birbaşa tamamlanmamalıdır."
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Conflict,
            invalidTransitionResponse.StatusCode);

        var invalidTransitionProblem =
            await invalidTransitionResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            "admin-orders.invalid-transition",
            invalidTransitionProblem
                .GetProperty("code")
                .GetString());

        using var confirmResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{orderId}" +
                "/status?lang=az",
                _fixture.OrderManagerAccessToken,
                new
                {
                    status = 2,
                    note =
                        "Sifariş admin tərəfindən təsdiqləndi."
                },
                cancellationToken);

        confirmResponse.EnsureSuccessStatusCode();

        var confirmedOrder =
            await confirmResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            2,
            confirmedOrder
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            JsonValueKind.String,
            confirmedOrder
                .GetProperty("confirmedAtUtc")
                .ValueKind);

        Assert.True(
            confirmedOrder
                .GetProperty("canBeCancelled")
                .GetBoolean());

        using var storePickupDeliveryResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{orderId}" +
                "/status?lang=az",
                _fixture.OrderManagerAccessToken,
                new
                {
                    status = 3,
                    note =
                        "StorePickup çatdırılmaya çıxarıla bilməz."
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Conflict,
            storePickupDeliveryResponse.StatusCode);

        const string cancellationReason =
            "Admin integration test sifarişi ləğv etdi.";

        using var cancelResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{orderId}" +
                "/status?lang=az",
                _fixture.OrderManagerAccessToken,
                new
                {
                    status = 5,
                    note =
                        cancellationReason
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
            JsonValueKind.String,
            cancelledOrder
                .GetProperty("cancelledAtUtc")
                .ValueKind);

        var statusHistory =
            cancelledOrder
                .GetProperty("statusHistory")
                .EnumerateArray()
                .ToArray();

        Assert.True(
            statusHistory.Length >= 3);

        var cancellationHistory =
            statusHistory[^1];

        Assert.Equal(
            2,
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

        using var repeatedChangeResponse =
            await SendAdminRequestAsync(
                HttpMethod.Patch,
                $"/api/admin/orders/{orderId}" +
                "/status?lang=az",
                _fixture.OrderManagerAccessToken,
                new
                {
                    status = 4,
                    note =
                        "Ləğv edilmiş sifariş dəyişdirilməməlidir."
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Conflict,
            repeatedChangeResponse.StatusCode);
    }

    private async Task EnsureBasketIsEmptyAsync(
        CancellationToken cancellationToken)
    {
        using var basketResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/basket?lang=az",
                cancellationToken);

        basketResponse.EnsureSuccessStatusCode();

        var basket =
            await basketResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var basketItemIds =
            basket
                .GetProperty("items")
                .EnumerateArray()
                .Select(
                    item =>
                        item
                            .GetProperty("id")
                            .GetGuid())
                .ToArray();

        foreach (var basketItemId in basketItemIds)
        {
            using var removeResponse =
                await _fixture.AuthenticatedClient
                    .DeleteAsync(
                        $"/api/basket/items/{basketItemId}" +
                        "?lang=az",
                        cancellationToken);

            removeResponse.EnsureSuccessStatusCode();
        }
    }

    private async Task<HttpResponseMessage>
        SendAdminRequestAsync(
            HttpMethod method,
            string requestUri,
            string accessToken,
            object? body,
            CancellationToken cancellationToken)
    {
        using var request =
            new HttpRequestMessage(
                method,
                requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        if (body is not null)
        {
            request.Content =
                JsonContent.Create(body);
        }

        return await _fixture.Client.SendAsync(
            request,
            cancellationToken);
    }
}
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task BasketEndpointsRequireAuthentication()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var response =
            await _fixture.Client.GetAsync(
                "/api/basket",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task BasketLifecycleWorks()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var initialResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/basket",
                cancellationToken);

        initialResponse.EnsureSuccessStatusCode();

        var initialBasket =
            await initialResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            "az",
            initialBasket
                .GetProperty("language")
                .GetString());

        Assert.Equal(
            0,
            initialBasket
                .GetProperty("totalQuantity")
                .GetInt32());

        Assert.False(
            initialBasket
                .GetProperty("canCheckout")
                .GetBoolean());

        Assert.Empty(
            initialBasket
                .GetProperty("items")
                .EnumerateArray());

        using var addResponse =
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

        addResponse.EnsureSuccessStatusCode();

        var addedBasket =
            await addResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            1,
            addedBasket
                .GetProperty("uniqueItemCount")
                .GetInt32());

        Assert.Equal(
            2,
            addedBasket
                .GetProperty("totalQuantity")
                .GetInt32());

        Assert.Equal(
            440m,
            addedBasket
                .GetProperty("subtotal")
                .GetDecimal());

        Assert.Equal(
            80.20m,
            addedBasket
                .GetProperty("discountAmount")
                .GetDecimal());

        Assert.Equal(
            359.80m,
            addedBasket
                .GetProperty("total")
                .GetDecimal());

        Assert.True(
            addedBasket
                .GetProperty("canCheckout")
                .GetBoolean());

        var addedItem =
            addedBasket
                .GetProperty("items")
                .EnumerateArray()
                .Single();

        var basketItemId =
            addedItem
                .GetProperty("id")
                .GetGuid();

        Assert.Equal(
            _fixture.ShoeVariantId,
            addedItem
                .GetProperty("productVariantId")
                .GetGuid());

        Assert.Equal(
            "Sarı adidas idman ayaqqabısı",
            addedItem
                .GetProperty("productName")
                .GetString());

        Assert.Equal(
            220m,
            addedItem
                .GetProperty("originalUnitPrice")
                .GetDecimal());

        Assert.Equal(
            179.90m,
            addedItem
                .GetProperty("unitPrice")
                .GetDecimal());

        Assert.Equal(
            2,
            addedItem
                .GetProperty("quantity")
                .GetInt32());

        Assert.Equal(
            "available",
            addedItem
                .GetProperty("availabilityCode")
                .GetString());

        using var updateResponse =
            await _fixture.AuthenticatedClient
                .PutAsJsonAsync(
                    $"/api/basket/items/{basketItemId}" +
                    "?lang=az",
                    new
                    {
                        quantity = 3
                    },
                    cancellationToken);

        updateResponse.EnsureSuccessStatusCode();

        var updatedBasket =
            await updateResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            3,
            updatedBasket
                .GetProperty("totalQuantity")
                .GetInt32());

        Assert.Equal(
            660m,
            updatedBasket
                .GetProperty("subtotal")
                .GetDecimal());

        Assert.Equal(
            120.30m,
            updatedBasket
                .GetProperty("discountAmount")
                .GetDecimal());

        Assert.Equal(
            539.70m,
            updatedBasket
                .GetProperty("total")
                .GetDecimal());

        using var removeResponse =
            await _fixture.AuthenticatedClient
                .DeleteAsync(
                    $"/api/basket/items/{basketItemId}" +
                    "?lang=az",
                    cancellationToken);

        removeResponse.EnsureSuccessStatusCode();

        var emptyBasket =
            await removeResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            0,
            emptyBasket
                .GetProperty("uniqueItemCount")
                .GetInt32());

        Assert.Equal(
            0,
            emptyBasket
                .GetProperty("totalQuantity")
                .GetInt32());

        Assert.Equal(
            0m,
            emptyBasket
                .GetProperty("total")
                .GetDecimal());

        Assert.False(
            emptyBasket
                .GetProperty("canCheckout")
                .GetBoolean());

        Assert.Empty(
            emptyBasket
                .GetProperty("items")
                .EnumerateArray());
    }

    [Fact]
    public async Task OutOfStockVariantCannotBeAdded()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var response =
            await _fixture.AuthenticatedClient
                .PostAsJsonAsync(
                    "/api/basket/items?lang=az",
                    new
                    {
                        productVariantId =
                            _fixture.OutOfStockVariantId,

                        quantity = 1
                    },
                    cancellationToken);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            "basket.out-of-stock",
            problem
                .GetProperty("code")
                .GetString());
    }
}
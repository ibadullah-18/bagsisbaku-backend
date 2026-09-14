using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
    : IClassFixture<CatalogApiFixture>
{
    private readonly CatalogApiFixture _fixture;

    public PublicCatalogApiTests(
        CatalogApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HealthEndpointReturnsBagsisbaku()
    {
        using var response =
            await _fixture.Client.GetAsync(
                "/health");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "bagsisbaku",
            body.GetProperty("name").GetString());

        Assert.Equal(
            "healthy",
            body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ProductsEndpointReturnsOnlySellableProducts()
    {
        using var response =
            await _fixture.Client.GetAsync(
                "/api/catalog/products" +
                "?language=az" +
                "&page=1" +
                "&pageSize=20");

        response.EnsureSuccessStatusCode();

        var body =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var items = body
            .GetProperty("items")
            .EnumerateArray()
            .ToArray();

        var productIds = items
            .Select(
                product =>
                    product
                        .GetProperty("id")
                        .GetGuid())
            .ToArray();

        Assert.Equal(
            2,
            body
                .GetProperty("totalCount")
                .GetInt32());

        Assert.Contains(
            _fixture.ShoeProductId,
            productIds);

        Assert.Contains(
            _fixture.BagProductId,
            productIds);

        Assert.DoesNotContain(
            _fixture.OutOfStockProductId,
            productIds);
    }

    [Fact]
    public async Task ProductDetailsReturnsRussianTranslations()
    {
        var requestUrl =
            $"/api/catalog/products/" +
            $"{_fixture.ShoeProductId}" +
            "?language=ru";

        using var response =
            await _fixture.Client.GetAsync(
                requestUrl);

        response.EnsureSuccessStatusCode();

        var body =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            _fixture.ShoeProductId,
            body.GetProperty("id").GetGuid());

        Assert.Equal(
            "ru",
            body.GetProperty("language").GetString());

        Assert.Equal(
            "Жёлтые кроссовки adidas",
            body.GetProperty("name").GetString());

        Assert.Equal(
            "Спортивная обувь",
            body
                .GetProperty("category")
                .GetProperty("name")
                .GetString());

        var variant = body
            .GetProperty("variants")
            .EnumerateArray()
            .Single();

        Assert.Equal(
            "Жёлтый",
            variant
                .GetProperty("color")
                .GetString());

        Assert.Equal(
            8,
            variant
                .GetProperty("stockCount")
                .GetInt32());
    }

    [Fact]
    public async Task MissingTranslationFallsBackToAzerbaijani()
    {
        var requestUrl =
            $"/api/catalog/products/" +
            $"{_fixture.BagProductId}" +
            "?language=en";

        using var response =
            await _fixture.Client.GetAsync(
                requestUrl);

        response.EnsureSuccessStatusCode();

        var body =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Qara adidas çanta",
            body.GetProperty("name").GetString());

        Assert.Equal(
            "Çantalar",
            body
                .GetProperty("category")
                .GetProperty("name")
                .GetString());

        var variant = body
            .GetProperty("variants")
            .EnumerateArray()
            .Single();

        Assert.Equal(
            "Standart",
            variant
                .GetProperty("size")
                .GetString());

        Assert.Equal(
            "Qara",
            variant
                .GetProperty("color")
                .GetString());
    }

    [Fact]
    public async Task FiltersEndpointReturnsShoeFilters()
    {
        using var response =
            await _fixture.Client.GetAsync(
                "/api/catalog/filters" +
                "?productType=1" +
                "&language=ru");

        response.EnsureSuccessStatusCode();

        var body =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        var brands = body
            .GetProperty("brands")
            .EnumerateArray()
            .ToArray();

        var categories = body
            .GetProperty("categories")
            .EnumerateArray()
            .ToArray();

        var sizes = body
            .GetProperty("sizes")
            .EnumerateArray()
            .ToArray();

        var colors = body
            .GetProperty("colors")
            .EnumerateArray()
            .ToArray();

        Assert.Single(brands);
        Assert.Single(categories);
        Assert.Single(sizes);
        Assert.Single(colors);

        Assert.Equal(
            "adidas",
            brands[0]
                .GetProperty("name")
                .GetString());

        Assert.Equal(
            "Спортивная обувь",
            categories[0]
                .GetProperty("name")
                .GetString());

        Assert.Equal(
            "Жёлтый",
            colors[0]
                .GetProperty("name")
                .GetString());
    }
}

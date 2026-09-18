using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task FavoriteEndpointsRequireAuthentication()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        var productId =
            _fixture.ShoeProductId;

        using var getResponse =
            await _fixture.Client.GetAsync(
                "/api/favorites?lang=az",
                cancellationToken);

        using var addResponse =
            await _fixture.Client.PostAsync(
                $"/api/favorites/{productId}?lang=az",
                content: null,
                cancellationToken);

        using var removeResponse =
            await _fixture.Client.DeleteAsync(
                $"/api/favorites/{productId}",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            getResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            addResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            removeResponse.StatusCode);
    }

    [Fact]
    public async Task FavoriteLifecycleWorks()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        var productId =
            _fixture.ShoeProductId;

        var favoriteUrl =
            $"/api/favorites/{productId}";

        try
        {
            using var initialCleanupResponse =
                await _fixture.AuthenticatedClient
                    .DeleteAsync(
                        favoriteUrl,
                        cancellationToken);

            Assert.Equal(
                HttpStatusCode.NoContent,
                initialCleanupResponse.StatusCode);

            using var addResponse =
                await _fixture.AuthenticatedClient
                    .PostAsync(
                        $"{favoriteUrl}?lang=az",
                        content: null,
                        cancellationToken);

            addResponse.EnsureSuccessStatusCode();

            var addedFavorite =
                await addResponse.Content
                    .ReadFromJsonAsync<JsonElement>(
                        cancellationToken);

            var favoriteId =
                addedFavorite
                    .GetProperty("favoriteId")
                    .GetGuid();

            Assert.NotEqual(
                Guid.Empty,
                favoriteId);

            Assert.Equal(
                productId,
                addedFavorite
                    .GetProperty("productId")
                    .GetGuid());

            Assert.Equal(
                "Sarı adidas idman ayaqqabısı",
                addedFavorite
                    .GetProperty("name")
                    .GetString());

            Assert.Equal(
                "BAGSIS-TEST-SHOE-001",
                addedFavorite
                    .GetProperty("productCode")
                    .GetString());

            Assert.Equal(
                "adidas",
                addedFavorite
                    .GetProperty("brandName")
                    .GetString());

            Assert.Equal(
                220m,
                addedFavorite
                    .GetProperty("price")
                    .GetDecimal());

            Assert.Equal(
                179.90m,
                addedFavorite
                    .GetProperty("currentPrice")
                    .GetDecimal());

            Assert.True(
                addedFavorite
                    .GetProperty("isDiscounted")
                    .GetBoolean());

            Assert.True(
                addedFavorite
                    .GetProperty("isAvailable")
                    .GetBoolean());

            using var duplicateAddResponse =
                await _fixture.AuthenticatedClient
                    .PostAsync(
                        $"{favoriteUrl}?lang=az",
                        content: null,
                        cancellationToken);

            duplicateAddResponse.EnsureSuccessStatusCode();

            var duplicateFavorite =
                await duplicateAddResponse.Content
                    .ReadFromJsonAsync<JsonElement>(
                        cancellationToken);

            Assert.Equal(
                favoriteId,
                duplicateFavorite
                    .GetProperty("favoriteId")
                    .GetGuid());

            using var listResponse =
                await _fixture.AuthenticatedClient
                    .GetAsync(
                        "/api/favorites?lang=en",
                        cancellationToken);

            listResponse.EnsureSuccessStatusCode();

            var favoriteList =
                await listResponse.Content
                    .ReadFromJsonAsync<JsonElement>(
                        cancellationToken);

            Assert.Equal(
                "en",
                favoriteList
                    .GetProperty("language")
                    .GetString());

            var listedFavorite =
                favoriteList
                    .GetProperty("items")
                    .EnumerateArray()
                    .Single(
                        item =>
                            item
                                .GetProperty("productId")
                                .GetGuid() ==
                            productId);

            Assert.Equal(
                favoriteId,
                listedFavorite
                    .GetProperty("favoriteId")
                    .GetGuid());

            Assert.Equal(
                "Yellow adidas shoes",
                listedFavorite
                    .GetProperty("name")
                    .GetString());

            Assert.Equal(
                179.90m,
                listedFavorite
                    .GetProperty("currentPrice")
                    .GetDecimal());

            using var removeResponse =
                await _fixture.AuthenticatedClient
                    .DeleteAsync(
                        favoriteUrl,
                        cancellationToken);

            Assert.Equal(
                HttpStatusCode.NoContent,
                removeResponse.StatusCode);

            using var repeatedRemoveResponse =
                await _fixture.AuthenticatedClient
                    .DeleteAsync(
                        favoriteUrl,
                        cancellationToken);

            Assert.Equal(
                HttpStatusCode.NoContent,
                repeatedRemoveResponse.StatusCode);

            using var finalListResponse =
                await _fixture.AuthenticatedClient
                    .GetAsync(
                        "/api/favorites?lang=az",
                        cancellationToken);

            finalListResponse.EnsureSuccessStatusCode();

            var finalFavoriteList =
                await finalListResponse.Content
                    .ReadFromJsonAsync<JsonElement>(
                        cancellationToken);

            var productIds =
                finalFavoriteList
                    .GetProperty("items")
                    .EnumerateArray()
                    .Select(
                        item =>
                            item
                                .GetProperty("productId")
                                .GetGuid())
                    .ToArray();

            Assert.DoesNotContain(
                productId,
                productIds);
        }
        finally
        {
            using var cleanupResponse =
                await _fixture.AuthenticatedClient
                    .DeleteAsync(
                        favoriteUrl,
                        CancellationToken.None);
        }
    }
}
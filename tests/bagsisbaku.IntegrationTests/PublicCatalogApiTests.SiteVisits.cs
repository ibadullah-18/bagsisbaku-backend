using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task TwoPageViewsFromOneVisitorIncreaseUniqueCountOnce()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var beforeResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/dashboard",
                _fixture.DashboardViewerAccessToken,
                body: null,
                cancellationToken);

        beforeResponse.EnsureSuccessStatusCode();

        var before =
            await beforeResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var previousViews =
            before.GetProperty("pageViews").GetInt32();

        var previousVisitors =
            before.GetProperty("uniqueVisitors").GetInt32();

        var visitorId =
            Guid.NewGuid();

        foreach (var pagePath in
            new[] { "/", "/products" })
        {
            using var visitResponse =
                await _fixture.Client
                    .PostAsJsonAsync(
                        "/api/visits",
                        new
                        {
                            visitorId,
                            pagePath
                        },
                        cancellationToken);

            Assert.Equal(
                HttpStatusCode.OK,
                visitResponse.StatusCode);
        }

        using var afterResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/dashboard",
                _fixture.DashboardViewerAccessToken,
                body: null,
                cancellationToken);

        afterResponse.EnsureSuccessStatusCode();

        var after =
            await afterResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            previousViews + 2,
            after.GetProperty("pageViews")
                .GetInt32());

        Assert.Equal(
            previousVisitors + 1,
            after.GetProperty("uniqueVisitors")
                .GetInt32());
    }
}
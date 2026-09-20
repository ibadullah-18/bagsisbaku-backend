using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task AdminDashboardRequiresPermissionAndReturnsMetrics()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var anonymousResponse =
            await _fixture.Client.GetAsync(
                "/api/admin/dashboard",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousResponse.StatusCode);

        using var unauthorizedAdminResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/dashboard",
                _fixture.OrderViewerAccessToken,
                body: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            unauthorizedAdminResponse.StatusCode);

        using var dashboardResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/dashboard",
                _fixture.DashboardViewerAccessToken,
                body: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            dashboardResponse.StatusCode);

        var dashboard =
            await dashboardResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.True(
            dashboard.GetProperty("productCount").GetInt32() >= 2);

        Assert.True(
            dashboard.GetProperty("pendingOrderCount").GetInt32() >= 0);

        Assert.True(
            dashboard.GetProperty("onDeliveryOrderCount").GetInt32() >= 0);

        Assert.True(
            dashboard.GetProperty("deliveredOrderCount").GetInt32() >= 0);

        Assert.True(
            dashboard.GetProperty("cancelledOrderCount").GetInt32() >= 0);

        Assert.True(
            dashboard.GetProperty("deliveredOrderTotal").GetDecimal() >= 0m);
    }
}
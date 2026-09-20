using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task WhatsAppInquiryIncrementsDashboardCount()
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

        var previousCount =
            before.GetProperty("whatsAppInquiryCount")
                .GetInt32();

        using var inquiryResponse =
            await _fixture.Client.PostAsync(
                $"/api/catalog/products/" +
                $"{_fixture.ShoeProductId}" +
                "/whatsapp-inquiry?lang=az",
                content: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            inquiryResponse.StatusCode);

        var inquiry =
            await inquiryResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.NotEqual(
            Guid.Empty,
            inquiry.GetProperty("inquiryId")
                .GetGuid());

        Assert.Equal(
            _fixture.ShoeProductId,
            inquiry.GetProperty("productId")
                .GetGuid());

        Assert.StartsWith(
            "https://wa.me/",
            inquiry.GetProperty("whatsAppUrl")
                .GetString());

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
            previousCount + 1,
            after.GetProperty("whatsAppInquiryCount")
                .GetInt32());
    }
}
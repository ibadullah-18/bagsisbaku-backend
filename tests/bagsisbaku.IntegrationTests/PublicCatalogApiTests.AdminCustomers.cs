using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task AdminCustomerEndpointsEnforcePermissions()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var anonymousResponse =
            await _fixture.Client.GetAsync(
                "/api/admin/customers",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousResponse.StatusCode);

        using var customerResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/admin/customers",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            customerResponse.StatusCode);

        using var viewerResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/customers" +
                "?page=1&pageSize=20",
                _fixture.CustomerViewerAccessToken,
                content: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.OK,
            viewerResponse.StatusCode);

        using var viewerStatusResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Put,
                $"/api/admin/customers/" +
                $"{_fixture.CustomerId}/status",
                _fixture.CustomerViewerAccessToken,
                JsonContent.Create(
                    new
                    {
                        isActive = false
                    }),
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            viewerStatusResponse.StatusCode);
    }

    [Fact]
    public async Task AdminCanViewAndManageCustomer()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var ensureActiveResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Put,
                $"/api/admin/customers/" +
                $"{_fixture.CustomerId}/status",
                _fixture.CustomerManagerAccessToken,
                JsonContent.Create(
                    new
                    {
                        isActive = true
                    }),
                cancellationToken);

        ensureActiveResponse.EnsureSuccessStatusCode();

        Assert.False(
            await _fixture
                .IsCustomerRefreshTokenRevokedAsync(
                    cancellationToken));

        using var listResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/customers" +
                "?search=Basket%20Test" +
                "&isActive=true" +
                "&emailConfirmed=true" +
                "&page=1" +
                "&pageSize=20",
                _fixture.CustomerManagerAccessToken,
                content: null,
                cancellationToken);

        listResponse.EnsureSuccessStatusCode();

        var page =
            await listResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            1,
            page
                .GetProperty("totalCount")
                .GetInt32());

        var customerSummary =
            page
                .GetProperty("items")
                .EnumerateArray()
                .Single();

        Assert.Equal(
            _fixture.CustomerId,
            customerSummary
                .GetProperty("id")
                .GetGuid());

        Assert.Equal(
            "Basket Test Customer",
            customerSummary
                .GetProperty("fullName")
                .GetString());

        Assert.True(
            customerSummary
                .GetProperty("isActive")
                .GetBoolean());

        using var detailsResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Get,
                $"/api/admin/customers/" +
                $"{_fixture.CustomerId}",
                _fixture.CustomerManagerAccessToken,
                content: null,
                cancellationToken);

        detailsResponse.EnsureSuccessStatusCode();

        var details =
            await detailsResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            _fixture.CustomerId,
            details
                .GetProperty("id")
                .GetGuid());

        Assert.True(
            details
                .GetProperty("isActive")
                .GetBoolean());

        Assert.True(
            details.TryGetProperty(
                "orderStatistics",
                out _));

        Assert.True(
            details.TryGetProperty(
                "addresses",
                out _));

        Assert.True(
            details.TryGetProperty(
                "recentOrders",
                out _));

        using var deactivateResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Put,
                $"/api/admin/customers/" +
                $"{_fixture.CustomerId}/status",
                _fixture.CustomerManagerAccessToken,
                JsonContent.Create(
                    new
                    {
                        isActive = false
                    }),
                cancellationToken);

        deactivateResponse.EnsureSuccessStatusCode();

        var deactivatedCustomer =
            await deactivateResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.False(
            deactivatedCustomer
                .GetProperty("isActive")
                .GetBoolean());

        Assert.True(
            await _fixture
                .IsCustomerRefreshTokenRevokedAsync(
                    cancellationToken));

        using var inactiveProfileResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/profile",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            inactiveProfileResponse.StatusCode);

        using var activateResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Put,
                $"/api/admin/customers/" +
                $"{_fixture.CustomerId}/status",
                _fixture.CustomerManagerAccessToken,
                JsonContent.Create(
                    new
                    {
                        isActive = true
                    }),
                cancellationToken);

        activateResponse.EnsureSuccessStatusCode();

        var activatedCustomer =
            await activateResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.True(
            activatedCustomer
                .GetProperty("isActive")
                .GetBoolean());

        using var activeProfileResponse =
            await _fixture.AuthenticatedClient.GetAsync(
                "/api/profile",
                cancellationToken);

        activeProfileResponse.EnsureSuccessStatusCode();

        Assert.True(
            await _fixture
                .IsCustomerRefreshTokenRevokedAsync(
                    cancellationToken));
    }

    [Fact]
    public async Task AdminCustomerFiltersValidateInput()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var invalidPageResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/customers" +
                "?page=0&pageSize=20",
                _fixture.CustomerManagerAccessToken,
                content: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            invalidPageResponse.StatusCode);

        using var invalidPageSizeResponse =
            await SendCustomerAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/customers" +
                "?page=1&pageSize=101",
                _fixture.CustomerManagerAccessToken,
                content: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            invalidPageSizeResponse.StatusCode);
    }

    private async Task<HttpResponseMessage>
        SendCustomerAdminRequestAsync(
            HttpMethod method,
            string requestUri,
            string accessToken,
            HttpContent? content,
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

        request.Content =
            content;

        return await _fixture.Client.SendAsync(
            request,
            cancellationToken);
    }
}
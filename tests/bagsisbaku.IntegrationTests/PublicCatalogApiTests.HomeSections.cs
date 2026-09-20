using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task AdminCanManageHomeSectionWithoutPublishingImageLessSection()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        using var anonymousResponse =
            await _fixture.Client.GetAsync(
                "/api/admin/home-sections",
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousResponse.StatusCode);

        using var forbiddenResponse =
            await SendAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/home-sections",
                _fixture.OrderViewerAccessToken,
                body: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            forbiddenResponse.StatusCode);

        using var createResponse =
            await SendAdminRequestAsync(
                HttpMethod.Post,
                "/api/admin/home-sections",
                _fixture.ContentManagerAccessToken,
                new
                {
                    type = 2,
                    title = "Test banner",
                    subtitle = "AZ mətn",
                    targetUrl = "/products",
                    sortOrder = 10,
                    translations = new[]
                    {
                        new
                        {
                            language = 2,
                            title = "Тестовый баннер",
                            subtitle = "RU mətn"
                        }
                    }
                },
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var sectionId =
            created.GetProperty("id").GetGuid();

        Assert.NotEqual(
            Guid.Empty,
            sectionId);

        Assert.False(
            created.GetProperty("isActive").GetBoolean());

        using var publicResponse =
            await _fixture.Client.GetAsync(
                "/api/home-sections?lang=ru",
                cancellationToken);

        publicResponse.EnsureSuccessStatusCode();

        var publicSections =
            await publicResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.DoesNotContain(
            publicSections.EnumerateArray(),
            section =>
                section.GetProperty("id").GetGuid() ==
                sectionId);

        using var updateResponse =
            await SendAdminRequestAsync(
                HttpMethod.Put,
                $"/api/admin/home-sections/{sectionId}",
                _fixture.ContentManagerAccessToken,
                new
                {
                    title = "Yenilənmiş banner",
                    subtitle = "Yeni AZ mətn",
                    targetUrl = "/products?type=1",
                    sortOrder = 2,
                    translations = new[]
                    {
                        new
                        {
                            language = 2,
                            title = "Обновлённый баннер",
                            subtitle = "Yeni RU mətn"
                        },
                        new
                        {
                            language = 3,
                            title = "Updated banner",
                            subtitle = "New EN text"
                        }
                    }
                },
                cancellationToken);

        updateResponse.EnsureSuccessStatusCode();

        var updated =
            await updateResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            "Yenilənmiş banner",
            updated.GetProperty("title").GetString());

        Assert.Equal(
            2,
            updated.GetProperty("translations")
                .GetArrayLength());

        using var deleteResponse =
            await SendAdminRequestAsync(
                HttpMethod.Delete,
                $"/api/admin/home-sections/{sectionId}",
                _fixture.ContentManagerAccessToken,
                body: null,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);
    }
}
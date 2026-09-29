using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class PublicCatalogApiTests
{
    [Fact]
    public async Task
        AdminCanQueueAndProcessEmailAnnouncement()
    {
        var cancellationToken =
            TestContext.Current.CancellationToken;

        _fixture.ClearSentAnnouncementEmails();

        var requestBody =
            new
            {
                subject =
                    "bagsisbaku integration test",

                htmlBody =
                    "<h1>bagsisbaku</h1>" +
                    "<p>Integration test emailidir.</p>",

                textBody =
                    "bagsisbaku integration test emailidir.",

                sendToAllCustomers =
                    false,

                customerIds =
                    Array.Empty<Guid>(),

                additionalEmails =
                    new[]
                    {
                        "customer1@example.com",
                        "customer2@example.com",
                        "customer3@example.com",
                        "customer4@example.com",
                        "customer5@example.com"
                    }
            };

        using var anonymousResponse =
            await _fixture.Client.PostAsJsonAsync(
                "/api/admin/email-announcements",
                requestBody,
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousResponse.StatusCode);

        using var forbiddenResponse =
            await SendAnnouncementAdminRequestAsync(
                HttpMethod.Post,
                "/api/admin/email-announcements",
                _fixture.OrderViewerAccessToken,
                JsonContent.Create(
                    requestBody),
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            forbiddenResponse.StatusCode);

        var announcementManagerToken =
            await _fixture
                .CreateAnnouncementManagerAccessTokenAsync();

        using var createResponse =
            await SendAnnouncementAdminRequestAsync(
                HttpMethod.Post,
                "/api/admin/email-announcements",
                announcementManagerToken,
                JsonContent.Create(
                    requestBody),
                cancellationToken);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var queuedAnnouncement =
            await createResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        var announcementId =
            queuedAnnouncement
                .GetProperty("id")
                .GetGuid();

        Assert.NotEqual(
            Guid.Empty,
            announcementId);

        Assert.Equal(
            2,
            queuedAnnouncement
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            5,
            queuedAnnouncement
                .GetProperty("recipientCount")
                .GetInt32());

        Assert.Equal(
            0,
            queuedAnnouncement
                .GetProperty("sentCount")
                .GetInt32());

        using var processResponse =
            await SendAnnouncementAdminRequestAsync(
                HttpMethod.Post,
                $"/api/admin/email-announcements/" +
                $"{announcementId}/process",
                announcementManagerToken,
                content: null,
                cancellationToken);

        processResponse.EnsureSuccessStatusCode();

        var completedAnnouncement =
            await processResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Equal(
            4,
            completedAnnouncement
                .GetProperty("status")
                .GetInt32());

        Assert.Equal(
            "Completed",
            completedAnnouncement
                .GetProperty("statusName")
                .GetString());

        Assert.Equal(
            5,
            completedAnnouncement
                .GetProperty("recipientCount")
                .GetInt32());

        Assert.Equal(
            5,
            completedAnnouncement
                .GetProperty("sentCount")
                .GetInt32());

        Assert.Equal(
            0,
            completedAnnouncement
                .GetProperty("failedCount")
                .GetInt32());

        var recipients =
            completedAnnouncement
                .GetProperty("recipients")
                .EnumerateArray()
                .ToArray();

        Assert.Equal(
            5,
            recipients.Length);

        Assert.All(
            recipients,
            recipient =>
            {
                Assert.Equal(
                    2,
                    recipient
                        .GetProperty("status")
                        .GetInt32());

                Assert.Equal(
                    1,
                    recipient
                        .GetProperty("attemptCount")
                        .GetInt32());

                Assert.Equal(
                    JsonValueKind.Null,
                    recipient
                        .GetProperty("failureReason")
                        .ValueKind);
            });

        var sentEmails =
            _fixture
                .GetSentAnnouncementEmails()
                .ToArray();

        Assert.Equal(
            5,
            sentEmails.Length);

        Assert.All(
            sentEmails,
            email =>
            {
                Assert.Equal(
                    "bagsisbaku integration test",
                    email.Subject);

                Assert.Contains(
                    "bagsisbaku",
                    email.HtmlBody,
                    StringComparison.OrdinalIgnoreCase);
            });

        using var detailResponse =
            await SendAnnouncementAdminRequestAsync(
                HttpMethod.Get,
                $"/api/admin/email-announcements/" +
                $"{announcementId}",
                announcementManagerToken,
                content: null,
                cancellationToken);

        detailResponse.EnsureSuccessStatusCode();

        using var listResponse =
            await SendAnnouncementAdminRequestAsync(
                HttpMethod.Get,
                "/api/admin/email-announcements" +
                "?page=1&pageSize=20",
                announcementManagerToken,
                content: null,
                cancellationToken);

        listResponse.EnsureSuccessStatusCode();

        var page =
            await listResponse.Content
                .ReadFromJsonAsync<JsonElement>(
                    cancellationToken);

        Assert.Contains(
            page
                .GetProperty("items")
                .EnumerateArray(),
            item =>
                item
                    .GetProperty("id")
                    .GetGuid() ==
                announcementId);
    }

    private async Task<HttpResponseMessage>
        SendAnnouncementAdminRequestAsync(
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
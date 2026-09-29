using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Email;
using bagsisbaku.Application.Security;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.IntegrationTests;

public sealed partial class CatalogApiFixture
{
    public async Task<string>
        CreateAnnouncementManagerAccessTokenAsync()
    {
        if (_factory is null)
        {
            throw new InvalidOperationException(
                "Integration test factory başladılmayıb.");
        }

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<AppUser>>();

        var administrators =
            await userManager.GetUsersInRoleAsync(
                SystemRoles.Admin);

        var administrator =
            administrators.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Test admin istifadəçisi tapılmadı.");

        var tokenGenerator =
            scope.ServiceProvider
                .GetRequiredService<
                    IAccessTokenGenerator>();

        var token =
            tokenGenerator.Generate(
                new AccessTokenUser(
                    administrator.Id,
                    administrator.Email!,
                    administrator.FullName,
                    [SystemRoles.Admin],
                    [
                        PermissionNames
                            .Announcements.Send
                    ]));

        return token.Token;
    }

    public void ClearSentAnnouncementEmails()
    {
        GetTestEmailSender()
            .Clear();
    }

    public IReadOnlyCollection<EmailMessage>
        GetSentAnnouncementEmails()
    {
        return GetTestEmailSender()
            .Messages;
    }

    private TestEmailSender GetTestEmailSender()
    {
        if (_factory is null)
        {
            throw new InvalidOperationException(
                "Integration test factory başladılmayıb.");
        }

        return _factory.Services
            .GetRequiredService<
                TestEmailSender>();
    }
}
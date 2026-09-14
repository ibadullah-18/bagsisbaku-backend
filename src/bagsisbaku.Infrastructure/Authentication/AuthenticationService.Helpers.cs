using System.Security.Cryptography;
using System.Text;
using bagsisbaku.Application.Authentication;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed partial class AuthenticationService
{
    private async Task<IReadOnlyCollection<string>>
        GetPermissionsAsync(
            AppUser user,
            IReadOnlyCollection<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        if (
            roles.Contains(
                SystemRoles.SuperAdmin,
                StringComparer.Ordinal)
        )
        {
            return PermissionNames.All
                .OrderBy(
                    permission => permission,
                    StringComparer.Ordinal)
                .ToArray();
        }

        var permissions =
            new HashSet<string>(
                StringComparer.Ordinal);

        // Adminə birbaşa verilmiş permission-lar.
        var userClaims =
            await _userManager.GetClaimsAsync(user);

        foreach (
            var claim in userClaims.Where(
                claim =>
                    claim.Type ==
                        CustomClaimTypes.Permission &&
                    PermissionNames.All.Contains(
                        claim.Value))
        )
        {
            permissions.Add(claim.Value);
        }

        // Gələcəkdə role permission istifadə edilərsə
        // onları da nəzərə alırıq.
        foreach (var roleName in roles)
        {
            var role =
                await _roleManager.FindByNameAsync(
                    roleName);

            if (role is null)
            {
                continue;
            }

            var roleClaims =
                await _roleManager.GetClaimsAsync(role);

            foreach (
                var claim in roleClaims.Where(
                    claim =>
                        claim.Type ==
                            CustomClaimTypes.Permission &&
                        PermissionNames.All.Contains(
                            claim.Value))
            )
            {
                permissions.Add(claim.Value);
            }
        }

        return permissions
            .OrderBy(
                permission => permission,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static Error ToAuthenticationError(
        IdentityResult identityResult)
    {
        ArgumentNullException.ThrowIfNull(
            identityResult);

        var descriptions =
            identityResult.Errors
                .Select(error => error.Description)
                .Where(
                    description =>
                        !string.IsNullOrWhiteSpace(
                            description))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        var description =
            descriptions.Length == 0
                ? "Identity əməliyyatı uğursuz oldu."
                : string.Join(" ", descriptions);

        return AuthenticationErrors.IdentityValidation(
            description);
    }

    private static string EncodeIdentityToken(
        string identityToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            identityToken);

        var tokenBytes =
            Encoding.UTF8.GetBytes(identityToken);

        try
        {
            return WebEncoders.Base64UrlEncode(
                tokenBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                tokenBytes);
        }
    }

    private static bool TryDecodeIdentityToken(
        string urlSafeToken,
        out string identityToken)
    {
        identityToken = string.Empty;

        if (string.IsNullOrWhiteSpace(urlSafeToken))
        {
            return false;
        }

        byte[] tokenBytes = [];

        try
        {
            tokenBytes =
                WebEncoders.Base64UrlDecode(
                    urlSafeToken);

            identityToken =
                Encoding.UTF8.GetString(
                    tokenBytes);

            return !string.IsNullOrWhiteSpace(
                identityToken);
        }
        catch (FormatException)
        {
            return false;
        }
        finally
        {
            if (tokenBytes.Length > 0)
            {
                CryptographicOperations.ZeroMemory(
                    tokenBytes);
            }
        }
    }
}

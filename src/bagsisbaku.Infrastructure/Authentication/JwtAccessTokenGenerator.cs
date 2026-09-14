using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Security;
using Microsoft.IdentityModel.Tokens;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed class JwtAccessTokenGenerator
    : IAccessTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly IClock _clock;
    private readonly SigningCredentials _signingCredentials;

    public JwtAccessTokenGenerator(
        JwtSettings settings,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(clock);

        settings.Validate();

        _settings = settings;
        _clock = clock;

        var securityKey =
            new SymmetricSecurityKey(
                Convert.FromBase64String(
                    settings.SigningKey));

        _signingCredentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);
    }

    public GeneratedAccessToken Generate(
        AccessTokenUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId boş ola bilməz.",
                nameof(user));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            user.Email);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            user.FullName);

        ArgumentNullException.ThrowIfNull(
            user.Roles);

        ArgumentNullException.ThrowIfNull(
            user.Permissions);

        var utcNow = _clock.UtcNow;

        var expiresAtUtc =
            utcNow.AddMinutes(
                _settings.AccessTokenMinutes);

        var tokenId =
            Guid.NewGuid().ToString("N");

        var userId =
            user.UserId.ToString();

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId),

            new(
                ClaimTypes.NameIdentifier,
                userId),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                JwtRegisteredClaimNames.Jti,
                tokenId),

            new(
                CustomClaimTypes.TokenId,
                tokenId)
        };

        var roles = user.Roles
            .Where(
                role =>
                    SystemRoles.IsDefined(role))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                role => role,
                StringComparer.Ordinal);

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var permissions = user.Permissions
            .Where(
                permission =>
                    PermissionNames.All.Contains(
                        permission))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                permission => permission,
                StringComparer.Ordinal);

        foreach (var permission in permissions)
        {
            claims.Add(
                new Claim(
                    CustomClaimTypes.Permission,
                    permission));
        }

        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: utcNow.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials:
                _signingCredentials);

        var token =
            new JwtSecurityTokenHandler()
                .WriteToken(jwt);

        return new GeneratedAccessToken(
            token,
            tokenId,
            expiresAtUtc);
    }
}

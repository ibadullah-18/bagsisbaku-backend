using System.Security.Claims;
using bagsisbaku.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace bagsisbaku.Infrastructure.Authentication;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        JwtSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.Validate();

        var signingKey =
            new SymmetricSecurityKey(
                Convert.FromBase64String(
                    settings.SigningKey));

        services.AddSingleton(settings);

        services.AddSingleton<
            IAccessTokenGenerator,
            JwtAccessTokenGenerator>();

        services.AddSingleton<
            IRefreshTokenGenerator,
            SecureRefreshTokenGenerator>();

        services
            .AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults
                            .AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults
                            .AuthenticationScheme;
                })
            .AddJwtBearer(
                options =>
                {
                    options.MapInboundClaims = false;
                    options.SaveToken = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,

                            ValidIssuer =
                                settings.Issuer,

                            ValidateAudience = true,

                            ValidAudience =
                                settings.Audience,

                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                signingKey,

                            ValidateLifetime = true,

                            RequireExpirationTime = true,

                            RequireSignedTokens = true,

                            ClockSkew =
                                TimeSpan.FromSeconds(30),

                            NameClaimType =
                                ClaimTypes.Name,

                            RoleClaimType =
                                ClaimTypes.Role
                        };
                });

        services.AddAuthorization();

        return services;
    }
}

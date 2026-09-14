using System.Security.Cryptography;

namespace bagsisbaku.Infrastructure.Authentication;

public sealed record JwtSettings(
    string Issuer,
    string Audience,
    string SigningKey,
    int AccessTokenMinutes,
    int RefreshTokenDays)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            Issuer);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            Audience);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            SigningKey);

        if (AccessTokenMinutes is < 5 or > 60)
        {
            throw new InvalidOperationException(
                "JWT access token müddəti 5–60 dəqiqə " +
                "arasında olmalıdır.");
        }

        if (RefreshTokenDays is < 1 or > 90)
        {
            throw new InvalidOperationException(
                "Refresh token müddəti 1–90 gün " +
                "arasında olmalıdır.");
        }

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(
                SigningKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey düzgün Base64 deyil.",
                exception);
        }

        try
        {
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:SigningKey minimum 256-bit " +
                    "olmalıdır.");
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                keyBytes);
        }
    }
}

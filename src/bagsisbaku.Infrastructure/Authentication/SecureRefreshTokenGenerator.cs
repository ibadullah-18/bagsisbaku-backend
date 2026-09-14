using System.Security.Cryptography;
using System.Text;
using bagsisbaku.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.WebUtilities;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed class SecureRefreshTokenGenerator
    : IRefreshTokenGenerator
{
    private const int TokenLengthInBytes = 64;

    public GeneratedRefreshToken Generate()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(
                TokenLengthInBytes);

        try
        {
            var token =
                WebEncoders.Base64UrlEncode(
                    randomBytes);

            return new GeneratedRefreshToken(
                token,
                Hash(token));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                randomBytes);
        }
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            token);

        var tokenBytes =
            Encoding.UTF8.GetBytes(token);

        try
        {
            var hashBytes =
                SHA256.HashData(tokenBytes);

            try
            {
                return Convert.ToHexString(
                    hashBytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(
                    hashBytes);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                tokenBytes);
        }
    }
}

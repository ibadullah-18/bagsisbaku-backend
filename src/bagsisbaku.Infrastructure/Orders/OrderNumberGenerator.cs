using System.Security.Cryptography;
using bagsisbaku.Application.Orders;

namespace bagsisbaku.Infrastructure.Orders;

internal sealed class OrderNumberGenerator
    : IOrderNumberGenerator
{
    public string Generate(
        DateTimeOffset utcNow)
    {
        var randomPart =
            RandomNumberGenerator
                .GetHexString(6)
                .ToLowerInvariant();

        return
            $"bagsisbaku-{utcNow:yyyyMMdd}-{randomPart}";
    }
}
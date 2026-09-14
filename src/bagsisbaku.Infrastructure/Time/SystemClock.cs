using bagsisbaku.Application.Abstractions.Time;

namespace bagsisbaku.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

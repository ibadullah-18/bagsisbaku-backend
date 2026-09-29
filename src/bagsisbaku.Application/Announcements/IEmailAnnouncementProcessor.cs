using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Announcements;

public interface IEmailAnnouncementProcessor
{
    Task<Result>
        ProcessAsync(
            Guid announcementId,
            CancellationToken cancellationToken = default);

    Task<int>
        ProcessPendingAsync(
            int batchSize,
            CancellationToken cancellationToken = default);
}
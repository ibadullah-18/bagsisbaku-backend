using bagsisbaku.Application.Common.Pagination;
using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Announcements;

public interface IEmailAnnouncementService
{
    Task<Result<EmailAnnouncementDetailsModel>>
        QueueAsync(
            QueueEmailAnnouncementCommand command,
            CancellationToken cancellationToken = default);

    Task<PagedResult<EmailAnnouncementSummaryModel>>
        GetAllAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

    Task<Result<EmailAnnouncementDetailsModel>>
        GetByIdAsync(
            Guid announcementId,
            CancellationToken cancellationToken = default);

    Task<Result>
        RetryFailedRecipientsAsync(
            Guid announcementId,
            CancellationToken cancellationToken = default);
}
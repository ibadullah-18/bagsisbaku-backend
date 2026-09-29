namespace bagsisbaku.Contracts.Announcements;

public sealed record EmailAnnouncementSummaryResponse(
    Guid Id,
    Guid CreatedByAdminId,
    string Subject,
    int Status,
    string StatusName,
    int RecipientCount,
    int SentCount,
    int FailedCount,
    DateTimeOffset? QueuedAtUtc,
    DateTimeOffset? ProcessingStartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc);

public sealed record EmailAnnouncementRecipientResponse(
    Guid Id,
    Guid? UserId,
    string Email,
    int Status,
    string StatusName,
    int AttemptCount,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? SentAtUtc,
    string? FailureReason);

public sealed record EmailAnnouncementDetailsResponse(
    Guid Id,
    Guid CreatedByAdminId,
    string Subject,
    string HtmlBody,
    string TextBody,
    int Status,
    string StatusName,
    int RecipientCount,
    int SentCount,
    int FailedCount,
    DateTimeOffset? QueuedAtUtc,
    DateTimeOffset? ProcessingStartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyCollection<
        EmailAnnouncementRecipientResponse> Recipients);

public sealed record EmailAnnouncementPageResponse(
    IReadOnlyCollection<
        EmailAnnouncementSummaryResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
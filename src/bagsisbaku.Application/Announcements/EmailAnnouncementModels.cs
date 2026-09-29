using bagsisbaku.Domain.Announcements;

namespace bagsisbaku.Application.Announcements;

public sealed record QueueEmailAnnouncementCommand(
    string Subject,
    string HtmlBody,
    string TextBody,
    bool SendToAllCustomers,
    IReadOnlyCollection<Guid> CustomerIds,
    IReadOnlyCollection<string> AdditionalEmails);

public sealed record EmailAnnouncementSummaryModel(
    Guid Id,
    Guid CreatedByAdminId,
    string Subject,
    EmailAnnouncementStatus Status,
    int RecipientCount,
    int SentCount,
    int FailedCount,
    DateTimeOffset? QueuedAtUtc,
    DateTimeOffset? ProcessingStartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc);

public sealed record EmailAnnouncementRecipientModel(
    Guid Id,
    Guid? UserId,
    string Email,
    EmailAnnouncementRecipientStatus Status,
    int AttemptCount,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? SentAtUtc,
    string? FailureReason);

public sealed record EmailAnnouncementDetailsModel(
    Guid Id,
    Guid CreatedByAdminId,
    string Subject,
    string HtmlBody,
    string TextBody,
    EmailAnnouncementStatus Status,
    int RecipientCount,
    int SentCount,
    int FailedCount,
    DateTimeOffset? QueuedAtUtc,
    DateTimeOffset? ProcessingStartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyCollection<EmailAnnouncementRecipientModel> Recipients);
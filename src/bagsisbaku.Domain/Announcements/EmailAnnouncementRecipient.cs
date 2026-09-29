using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Announcements;

public sealed class EmailAnnouncementRecipient
    : AuditableEntity
{
    private EmailAnnouncementRecipient()
    {
    }

    private EmailAnnouncementRecipient(
        Guid id,
        Guid announcementId,
        Guid? userId,
        string email)
        : base(id)
    {
        AnnouncementId =
            DomainGuard.NotEmpty(
                announcementId,
                nameof(AnnouncementId));

        if (userId == Guid.Empty)
        {
            throw new DomainException(
                "İstifadəçi ID-si boş ola bilməz.");
        }

        UserId = userId;

        Email =
            DomainGuard.Required(
                email,
                nameof(Email),
                256);

        Status =
            EmailAnnouncementRecipientStatus.Pending;
    }

    public Guid AnnouncementId
    {
        get;
        private set;
    }

    public Guid? UserId
    {
        get;
        private set;
    }

    public string Email
    {
        get;
        private set;
    } = string.Empty;

    public EmailAnnouncementRecipientStatus Status
    {
        get;
        private set;
    }

    public int AttemptCount
    {
        get;
        private set;
    }

    public DateTimeOffset? LastAttemptAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? SentAtUtc
    {
        get;
        private set;
    }

    public string? FailureReason
    {
        get;
        private set;
    }

    public static EmailAnnouncementRecipient Create(
        Guid announcementId,
        Guid? userId,
        string email)
    {
        return new EmailAnnouncementRecipient(
            Guid.NewGuid(),
            announcementId,
            userId,
            email);
    }

    public void MarkSent(
        DateTimeOffset sentAtUtc)
    {
        if (Status ==
            EmailAnnouncementRecipientStatus.Sent)
        {
            return;
        }

        AttemptCount++;
        LastAttemptAtUtc = sentAtUtc;
        SentAtUtc = sentAtUtc;
        FailureReason = null;

        Status =
            EmailAnnouncementRecipientStatus.Sent;
    }

    public void MarkFailed(
        string failureReason,
        DateTimeOffset failedAtUtc)
    {
        if (Status ==
            EmailAnnouncementRecipientStatus.Sent)
        {
            throw new DomainException(
                "Göndərilmiş email uğursuz kimi işarələnə bilməz.");
        }

        AttemptCount++;
        LastAttemptAtUtc = failedAtUtc;
        SentAtUtc = null;

        FailureReason =
            DomainGuard.Required(
                failureReason,
                nameof(FailureReason),
                2000);

        Status =
            EmailAnnouncementRecipientStatus.Failed;
    }

    public void Retry()
    {
        if (Status !=
            EmailAnnouncementRecipientStatus.Failed)
        {
            throw new DomainException(
                "Yalnız uğursuz email yenidən növbəyə qaytarıla bilər.");
        }

        FailureReason = null;

        Status =
            EmailAnnouncementRecipientStatus.Pending;
    }
}
using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Announcements;

public sealed class EmailAnnouncement : AuditableEntity
{
    private EmailAnnouncement()
    {
    }

    private EmailAnnouncement(
        Guid id,
        Guid createdByAdminId,
        string subject,
        string htmlBody,
        string textBody)
        : base(id)
    {
        CreatedByAdminId =
            DomainGuard.NotEmpty(
                createdByAdminId,
                nameof(CreatedByAdminId));

        Subject =
            DomainGuard.Required(
                subject,
                nameof(Subject),
                200);

        HtmlBody =
            DomainGuard.Required(
                htmlBody,
                nameof(HtmlBody),
                20_000);

        TextBody =
            DomainGuard.Required(
                textBody,
                nameof(TextBody),
                10_000);

        Status =
            EmailAnnouncementStatus.Draft;
    }

    public Guid CreatedByAdminId
    {
        get;
        private set;
    }

    public string Subject
    {
        get;
        private set;
    } = string.Empty;

    public string HtmlBody
    {
        get;
        private set;
    } = string.Empty;

    public string TextBody
    {
        get;
        private set;
    } = string.Empty;

    public EmailAnnouncementStatus Status
    {
        get;
        private set;
    }

    public int RecipientCount
    {
        get;
        private set;
    }

    public int SentCount
    {
        get;
        private set;
    }

    public int FailedCount
    {
        get;
        private set;
    }

    public DateTimeOffset? QueuedAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? ProcessingStartedAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? CompletedAtUtc
    {
        get;
        private set;
    }

    public static EmailAnnouncement Create(
        Guid createdByAdminId,
        string subject,
        string htmlBody,
        string textBody)
    {
        return new EmailAnnouncement(
            Guid.NewGuid(),
            createdByAdminId,
            subject,
            htmlBody,
            textBody);
    }

    public void Queue(
        int recipientCount,
        DateTimeOffset queuedAtUtc)
    {
        if (Status != EmailAnnouncementStatus.Draft)
        {
            throw new DomainException(
                "Yalnız qaralama elan növbəyə əlavə edilə bilər.");
        }

        if (recipientCount <= 0)
        {
            throw new DomainException(
                "Email elanının ən azı bir alıcısı olmalıdır.");
        }

        RecipientCount = recipientCount;
        SentCount = 0;
        FailedCount = 0;
        QueuedAtUtc = queuedAtUtc;
        Status = EmailAnnouncementStatus.Queued;
    }

    public void RequeueFailedRecipients(
        DateTimeOffset queuedAtUtc)
    {
        if (Status is not (
            EmailAnnouncementStatus.Failed or
            EmailAnnouncementStatus.PartiallyFailed))
        {
            throw new DomainException(
                "Yalnız uğursuz alıcıları olan elan yenidən növbəyə əlavə edilə bilər.");
        }

        if (FailedCount <= 0)
        {
            throw new DomainException(
                "Yenidən göndəriləcək uğursuz alıcı yoxdur.");
        }

        FailedCount = 0;
        QueuedAtUtc = queuedAtUtc;
        ProcessingStartedAtUtc = null;
        CompletedAtUtc = null;

        Status =
            EmailAnnouncementStatus.Queued;
    }
    public void StartProcessing(
        DateTimeOffset startedAtUtc)
    {
        if (Status != EmailAnnouncementStatus.Queued)
        {
            throw new DomainException(
                "Yalnız növbədə olan elan göndərilməyə başlana bilər.");
        }

        ProcessingStartedAtUtc = startedAtUtc;
        Status = EmailAnnouncementStatus.Processing;
    }

    public void RegisterSent()
    {
        EnsureProcessing();

        SentCount++;

        EnsureProcessedCountIsValid();
    }

    public void RegisterFailure()
    {
        EnsureProcessing();

        FailedCount++;

        EnsureProcessedCountIsValid();
    }

    public void Complete(
        DateTimeOffset completedAtUtc)
    {
        EnsureProcessing();

        if (SentCount + FailedCount != RecipientCount)
        {
            throw new DomainException(
                "Bütün alıcılar işlənmədən elan tamamlana bilməz.");
        }

        CompletedAtUtc = completedAtUtc;

        Status =
            FailedCount == 0
                ? EmailAnnouncementStatus.Completed
                : SentCount == 0
                    ? EmailAnnouncementStatus.Failed
                    : EmailAnnouncementStatus.PartiallyFailed;
    }

    private void EnsureProcessing()
    {
        if (Status !=
            EmailAnnouncementStatus.Processing)
        {
            throw new DomainException(
                "Elan hazırda göndərilmə mərhələsində deyil.");
        }
    }

    private void EnsureProcessedCountIsValid()
    {
        if (SentCount + FailedCount >
            RecipientCount)
        {
            throw new DomainException(
                "İşlənmiş alıcı sayı ümumi alıcı sayını keçə bilməz.");
        }
    }
}
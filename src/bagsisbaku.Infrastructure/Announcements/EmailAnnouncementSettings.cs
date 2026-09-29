using System.Net.Mail;

namespace bagsisbaku.Infrastructure.Announcements;

public sealed record EmailAnnouncementSettings(
    bool DemoMode,
    string? DemoRecipientEmail,
    int BatchSize,
    bool ProcessingEnabled,
    int ProcessingIntervalSeconds)
{
    public void Validate()
    {
        if (BatchSize is < 1 or > 100)
        {
            throw new InvalidOperationException(
                "EmailAnnouncements:BatchSize 1-100 arasında olmalıdır.");
        }

        if (ProcessingIntervalSeconds is < 2 or > 3600)
        {
            throw new InvalidOperationException(
                "EmailAnnouncements:ProcessingIntervalSeconds 2-3600 arasında olmalıdır.");
        }

        if (!DemoMode)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
            DemoRecipientEmail) ||
            !MailAddress.TryCreate(
                DemoRecipientEmail,
                out _))
        {
            throw new InvalidOperationException(
                "Demo rejimi üçün EmailAnnouncements:DemoRecipientEmail düzgün yazılmalıdır.");
        }
    }
}
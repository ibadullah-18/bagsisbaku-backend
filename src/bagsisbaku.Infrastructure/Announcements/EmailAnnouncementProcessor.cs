using bagsisbaku.Application.Abstractions.Email;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Announcements;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Announcements;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace bagsisbaku.Infrastructure.Announcements;

internal sealed class EmailAnnouncementProcessor(
    ApplicationDbContext dbContext,
    IEmailSender emailSender,
    IClock clock,
    EmailAnnouncementSettings settings,
    ILogger<EmailAnnouncementProcessor> logger)
    : IEmailAnnouncementProcessor
{
    public async Task<Result> ProcessAsync(
        Guid announcementId,
        CancellationToken cancellationToken = default)
    {
        var announcement =
            await dbContext.EmailAnnouncements
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == announcementId,
                    cancellationToken);

        if (announcement is null)
        {
            return Result.Failure(
                EmailAnnouncementErrors.NotFound);
        }

        if (announcement.Status !=
            EmailAnnouncementStatus.Queued)
        {
            return Result.Failure(
                EmailAnnouncementErrors
                    .AlreadyProcessing);
        }

        var recipients =
            await dbContext.EmailAnnouncementRecipients
                .Where(
                    recipient =>
                        recipient.AnnouncementId ==
                            announcementId &&
                        recipient.Status ==
                            EmailAnnouncementRecipientStatus
                                .Pending)
                .OrderBy(
                    recipient =>
                        recipient.CreatedAtUtc)
                .ToArrayAsync(
                    cancellationToken);

        announcement.StartProcessing(
            clock.UtcNow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        foreach (var recipient in recipients)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            try
            {
                var destinationEmail =
                    settings.DemoMode
                        ? settings.DemoRecipientEmail!
                        : recipient.Email;

                var subject =
                    settings.DemoMode
                        ? $"[DEMO → {recipient.Email}] " +
                          announcement.Subject
                        : announcement.Subject;

                var message =
                    new EmailMessage(
                        destinationEmail,
                        subject,
                        announcement.HtmlBody,
                        announcement.TextBody);

                await emailSender.SendAsync(
                    message,
                    cancellationToken);

                var sentAtUtc =
                    clock.UtcNow;

                recipient.MarkSent(
                    sentAtUtc);

                announcement.RegisterSent();

                logger.LogInformation(
                    "Email announcement recipient sent. AnnouncementId: {AnnouncementId}; RecipientId: {RecipientId}; OriginalEmail: {OriginalEmail}; DestinationEmail: {DestinationEmail}",
                    announcement.Id,
                    recipient.Id,
                    recipient.Email,
                    destinationEmail);
            }
            catch (OperationCanceledException)
                when (cancellationToken
                    .IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var failureReason =
                    NormalizeFailureReason(
                        exception.Message);

                recipient.MarkFailed(
                    failureReason,
                    clock.UtcNow);

                announcement.RegisterFailure();

                logger.LogError(
                    exception,
                    "Email announcement recipient failed. AnnouncementId: {AnnouncementId}; RecipientId: {RecipientId}; Email: {Email}",
                    announcement.Id,
                    recipient.Id,
                    recipient.Email);
            }

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        announcement.Complete(
            clock.UtcNow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    public async Task<int> ProcessPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedBatchSize =
            Math.Clamp(
                batchSize,
                1,
                100);

        var announcementIds =
            await dbContext.EmailAnnouncements
                .AsNoTracking()
                .Where(
                    announcement =>
                        announcement.Status ==
                        EmailAnnouncementStatus.Queued)
                .OrderBy(
                    announcement =>
                        announcement.QueuedAtUtc)
                .ThenBy(
                    announcement =>
                        announcement.CreatedAtUtc)
                .Select(
                    announcement =>
                        announcement.Id)
                .Take(normalizedBatchSize)
                .ToArrayAsync(
                    cancellationToken);

        var processedCount = 0;

        foreach (
            var announcementId in
            announcementIds)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            var result =
                await ProcessAsync(
                    announcementId,
                    cancellationToken);

            if (result.IsSuccess)
            {
                processedCount++;
            }
        }

        return processedCount;
    }

    private static string NormalizeFailureReason(
        string? failureReason)
    {
        const string fallbackMessage =
            "Email göndərilərkən naməlum xəta baş verdi.";

        var normalized =
            string.IsNullOrWhiteSpace(
                failureReason)
                ? fallbackMessage
                : failureReason.Trim();

        return normalized.Length <= 2000
            ? normalized
            : normalized[..2000];
    }
}
using System.Net.Mail;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Announcements;
using bagsisbaku.Application.Common.Pagination;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;
using bagsisbaku.Domain.Announcements;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Announcements;

internal sealed class EmailAnnouncementService(
    ApplicationDbContext dbContext,
    UserManager<AppUser> userManager,
    ICurrentUser currentUser,
    IClock clock)
    : IEmailAnnouncementService
{
    public async Task<Result<EmailAnnouncementDetailsModel>>
        QueueAsync(
            QueueEmailAnnouncementCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var authorizationError =
            GetAuthorizationError();

        if (authorizationError is not null)
        {
            return Result.Failure<
                EmailAnnouncementDetailsModel>(
                    authorizationError);
        }

        var validationError =
            ValidateCommand(command);

        if (validationError is not null)
        {
            return Result.Failure<
                EmailAnnouncementDetailsModel>(
                    validationError);
        }

        var administratorId =
            currentUser.UserId!.Value;

        var customerUsers =
            await userManager.GetUsersInRoleAsync(
                SystemRoles.Customer);

        var activeCustomers =
            customerUsers
                .Where(
                    user =>
                        user.IsActive &&
                        user.EmailConfirmed &&
                        !string.IsNullOrWhiteSpace(
                            user.Email))
                .ToArray();

        var selectedCustomerIds =
            (command.CustomerIds ??
                Array.Empty<Guid>())
            .Where(customerId =>
                customerId != Guid.Empty)
            .Distinct()
            .ToArray();

        AppUser[] selectedCustomers;

        if (command.SendToAllCustomers)
        {
            selectedCustomers =
                activeCustomers;
        }
        else
        {
            var activeCustomerIds =
                activeCustomers
                    .Select(customer => customer.Id)
                    .ToHashSet();

            if (selectedCustomerIds.Any(
                customerId =>
                    !activeCustomerIds.Contains(
                        customerId)))
            {
                return Result.Failure<
                    EmailAnnouncementDetailsModel>(
                        EmailAnnouncementErrors
                            .CustomerNotFound);
            }

            selectedCustomers =
                activeCustomers
                    .Where(customer =>
                        selectedCustomerIds.Contains(
                            customer.Id))
                    .ToArray();
        }

        var recipientCandidates =
            new List<RecipientCandidate>();

        recipientCandidates.AddRange(
            selectedCustomers.Select(
                customer =>
                    new RecipientCandidate(
                        customer.Id,
                        customer.Email!)));

        foreach (
            var additionalEmail in
            command.AdditionalEmails ??
            Array.Empty<string>())
        {
            var normalizedEmail =
                additionalEmail?.Trim();

            if (string.IsNullOrWhiteSpace(
                normalizedEmail))
            {
                continue;
            }

            if (!MailAddress.TryCreate(
                normalizedEmail,
                out var parsedAddress))
            {
                return Result.Failure<
                    EmailAnnouncementDetailsModel>(
                        EmailAnnouncementErrors
                            .InvalidEmail);
            }

            recipientCandidates.Add(
                new RecipientCandidate(
                    null,
                    parsedAddress.Address));
        }

        var recipients =
            recipientCandidates
                .GroupBy(
                    recipient =>
                        recipient.Email,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();

        if (recipients.Length == 0)
        {
            return Result.Failure<
                EmailAnnouncementDetailsModel>(
                    EmailAnnouncementErrors
                        .RecipientRequired);
        }

        var announcement =
            EmailAnnouncement.Create(
                administratorId,
                command.Subject.Trim(),
                command.HtmlBody.Trim(),
                command.TextBody.Trim());

        var recipientEntities =
            recipients
                .Select(
                    recipient =>
                        EmailAnnouncementRecipient.Create(
                            announcement.Id,
                            recipient.UserId,
                            recipient.Email))
                .ToArray();

        announcement.Queue(
            recipientEntities.Length,
            clock.UtcNow);

        await dbContext.EmailAnnouncements.AddAsync(
            announcement,
            cancellationToken);

        await dbContext.EmailAnnouncementRecipients
            .AddRangeAsync(
                recipientEntities,
                cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            ToDetailsModel(
                announcement,
                recipientEntities));
    }

    public async Task<
        PagedResult<EmailAnnouncementSummaryModel>>
        GetAllAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        pageNumber =
            Math.Max(pageNumber, 1);

        pageSize =
            Math.Clamp(
                pageSize,
                1,
                100);

        var query =
            dbContext.EmailAnnouncements
                .AsNoTracking()
                .OrderByDescending(
                    announcement =>
                        announcement.CreatedAtUtc);

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var announcements =
            await query
                .Skip(
                    (pageNumber - 1) *
                    pageSize)
                .Take(pageSize)
                .Select(
                    announcement =>
                        new EmailAnnouncementSummaryModel(
                            announcement.Id,
                            announcement.CreatedByAdminId,
                            announcement.Subject,
                            announcement.Status,
                            announcement.RecipientCount,
                            announcement.SentCount,
                            announcement.FailedCount,
                            announcement.QueuedAtUtc,
                            announcement
                                .ProcessingStartedAtUtc,
                            announcement.CompletedAtUtc,
                            announcement.CreatedAtUtc))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<
            EmailAnnouncementSummaryModel>.Create(
                announcements,
                pageNumber,
                pageSize,
                totalCount);
    }

    public async Task<
        Result<EmailAnnouncementDetailsModel>>
        GetByIdAsync(
            Guid announcementId,
            CancellationToken cancellationToken = default)
    {
        var announcement =
            await dbContext.EmailAnnouncements
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == announcementId,
                    cancellationToken);

        if (announcement is null)
        {
            return Result.Failure<
                EmailAnnouncementDetailsModel>(
                    EmailAnnouncementErrors.NotFound);
        }

        var recipients =
            await dbContext.EmailAnnouncementRecipients
                .AsNoTracking()
                .Where(
                    recipient =>
                        recipient.AnnouncementId ==
                        announcementId)
                .OrderBy(
                    recipient =>
                        recipient.Email)
                .ToArrayAsync(
                    cancellationToken);

        return Result.Success(
            ToDetailsModel(
                announcement,
                recipients));
    }

    public async Task<Result>
        RetryFailedRecipientsAsync(
            Guid announcementId,
            CancellationToken cancellationToken = default)
    {
        var authorizationError =
            GetAuthorizationError();

        if (authorizationError is not null)
        {
            return Result.Failure(
                authorizationError);
        }

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

        var failedRecipients =
            await dbContext.EmailAnnouncementRecipients
                .Where(
                    recipient =>
                        recipient.AnnouncementId ==
                            announcementId &&
                        recipient.Status ==
                            EmailAnnouncementRecipientStatus
                                .Failed)
                .ToArrayAsync(
                    cancellationToken);

        if (failedRecipients.Length == 0)
        {
            return Result.Failure(
                EmailAnnouncementErrors
                    .NoFailedRecipients);
        }

        foreach (var recipient in failedRecipients)
        {
            recipient.Retry();
        }

        announcement.RequeueFailedRecipients(
            clock.UtcNow);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    private Error? GetAuthorizationError()
    {
        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is null)
        {
            return EmailAnnouncementErrors
                .AuthenticationRequired;
        }

        if (!currentUser.IsSuperAdmin &&
            !currentUser.HasPermission(
                PermissionNames.Announcements.Send))
        {
            return EmailAnnouncementErrors
                .AdministratorRequired;
        }

        return null;
    }

    private static Error? ValidateCommand(
        QueueEmailAnnouncementCommand command)
    {
        if (string.IsNullOrWhiteSpace(
            command.Subject))
        {
            return EmailAnnouncementErrors
                .SubjectRequired;
        }

        if (command.Subject.Trim().Length > 200)
        {
            return Error.Validation(
                "email-announcements.subject-too-long",
                "Email başlığı maksimum 200 simvol ola bilər.");
        }

        if (string.IsNullOrWhiteSpace(
            command.HtmlBody))
        {
            return EmailAnnouncementErrors
                .HtmlBodyRequired;
        }

        if (command.HtmlBody.Trim().Length >
            20_000)
        {
            return Error.Validation(
                "email-announcements.html-body-too-long",
                "Email HTML məzmunu maksimum 20000 simvol ola bilər.");
        }

        if (string.IsNullOrWhiteSpace(
            command.TextBody))
        {
            return EmailAnnouncementErrors
                .TextBodyRequired;
        }

        if (command.TextBody.Trim().Length >
            10_000)
        {
            return Error.Validation(
                "email-announcements.text-body-too-long",
                "Email mətn versiyası maksimum 10000 simvol ola bilər.");
        }

        return null;
    }

    private static EmailAnnouncementDetailsModel
        ToDetailsModel(
            EmailAnnouncement announcement,
            IReadOnlyCollection<
                EmailAnnouncementRecipient> recipients)
    {
        var recipientModels =
            recipients
                .Select(
                    recipient =>
                        new EmailAnnouncementRecipientModel(
                            recipient.Id,
                            recipient.UserId,
                            recipient.Email,
                            recipient.Status,
                            recipient.AttemptCount,
                            recipient.LastAttemptAtUtc,
                            recipient.SentAtUtc,
                            recipient.FailureReason))
                .ToArray();

        return new EmailAnnouncementDetailsModel(
            announcement.Id,
            announcement.CreatedByAdminId,
            announcement.Subject,
            announcement.HtmlBody,
            announcement.TextBody,
            announcement.Status,
            announcement.RecipientCount,
            announcement.SentCount,
            announcement.FailedCount,
            announcement.QueuedAtUtc,
            announcement.ProcessingStartedAtUtc,
            announcement.CompletedAtUtc,
            announcement.CreatedAtUtc,
            announcement.UpdatedAtUtc,
            recipientModels);
    }

    private sealed record RecipientCandidate(
        Guid? UserId,
        string Email);
}
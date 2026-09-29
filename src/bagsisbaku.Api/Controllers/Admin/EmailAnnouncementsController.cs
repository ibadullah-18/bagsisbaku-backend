using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Announcements;
using bagsisbaku.Application.Security;
using bagsisbaku.Contracts.Announcements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[HasPermission(PermissionNames.Announcements.Send)]
[Route("api/admin/email-announcements")]
public sealed class EmailAnnouncementsController(
    IEmailAnnouncementService announcementService,
    IEmailAnnouncementProcessor announcementProcessor)
    : ControllerBase
{
    [HttpPost]
    public async Task<
        ActionResult<EmailAnnouncementDetailsResponse>>
        QueueAsync(
            [FromBody]
            QueueEmailAnnouncementRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new QueueEmailAnnouncementCommand(
                request.Subject,
                request.HtmlBody,
                request.TextBody,
                request.SendToAllCustomers,
                request.CustomerIds ??
                    Array.Empty<Guid>(),
                request.AdditionalEmails ??
                    Array.Empty<string>());

        var result =
            await announcementService.QueueAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Created(
            $"/api/admin/email-announcements/" +
            $"{result.Value.Id}",
            ToDetailsResponse(
                result.Value));
    }

    [HttpGet]
    public async Task<
        ActionResult<EmailAnnouncementPageResponse>>
        GetAllAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var result =
            await announcementService.GetAllAsync(
                page,
                pageSize,
                cancellationToken);

        var response =
            new EmailAnnouncementPageResponse(
                result.Items
                    .Select(ToSummaryResponse)
                    .ToArray(),
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage);

        return Ok(response);
    }

    [HttpGet("{announcementId:guid}")]
    public async Task<
        ActionResult<EmailAnnouncementDetailsResponse>>
        GetByIdAsync(
            Guid announcementId,
            CancellationToken cancellationToken)
    {
        var result =
            await announcementService.GetByIdAsync(
                announcementId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(
                result.Value));
    }

    [HttpPost("{announcementId:guid}/process")]
    public async Task<
        ActionResult<EmailAnnouncementDetailsResponse>>
        ProcessAsync(
            Guid announcementId,
            CancellationToken cancellationToken)
    {
        var processResult =
            await announcementProcessor.ProcessAsync(
                announcementId,
                cancellationToken);

        if (processResult.IsFailure)
        {
            return this.ToProblemResult(
                processResult.Error);
        }

        var result =
            await announcementService.GetByIdAsync(
                announcementId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(
                result.Value));
    }

    [HttpPost("{announcementId:guid}/retry")]
    public async Task<
        ActionResult<EmailAnnouncementDetailsResponse>>
        RetryAsync(
            Guid announcementId,
            CancellationToken cancellationToken)
    {
        var retryResult =
            await announcementService
                .RetryFailedRecipientsAsync(
                    announcementId,
                    cancellationToken);

        if (retryResult.IsFailure)
        {
            return this.ToProblemResult(
                retryResult.Error);
        }

        var processResult =
            await announcementProcessor.ProcessAsync(
                announcementId,
                cancellationToken);

        if (processResult.IsFailure)
        {
            return this.ToProblemResult(
                processResult.Error);
        }

        var result =
            await announcementService.GetByIdAsync(
                announcementId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(
                result.Value));
    }

    private static EmailAnnouncementSummaryResponse
        ToSummaryResponse(
            EmailAnnouncementSummaryModel model)
    {
        return new EmailAnnouncementSummaryResponse(
            model.Id,
            model.CreatedByAdminId,
            model.Subject,
            (int)model.Status,
            model.Status.ToString(),
            model.RecipientCount,
            model.SentCount,
            model.FailedCount,
            model.QueuedAtUtc,
            model.ProcessingStartedAtUtc,
            model.CompletedAtUtc,
            model.CreatedAtUtc);
    }

    private static EmailAnnouncementDetailsResponse
        ToDetailsResponse(
            EmailAnnouncementDetailsModel model)
    {
        var recipients =
            model.Recipients
                .Select(
                    recipient =>
                        new EmailAnnouncementRecipientResponse(
                            recipient.Id,
                            recipient.UserId,
                            recipient.Email,
                            (int)recipient.Status,
                            recipient.Status.ToString(),
                            recipient.AttemptCount,
                            recipient.LastAttemptAtUtc,
                            recipient.SentAtUtc,
                            recipient.FailureReason))
                .ToArray();

        return new EmailAnnouncementDetailsResponse(
            model.Id,
            model.CreatedByAdminId,
            model.Subject,
            model.HtmlBody,
            model.TextBody,
            (int)model.Status,
            model.Status.ToString(),
            model.RecipientCount,
            model.SentCount,
            model.FailedCount,
            model.QueuedAtUtc,
            model.ProcessingStartedAtUtc,
            model.CompletedAtUtc,
            model.CreatedAtUtc,
            model.UpdatedAtUtc,
            recipients);
    }
}
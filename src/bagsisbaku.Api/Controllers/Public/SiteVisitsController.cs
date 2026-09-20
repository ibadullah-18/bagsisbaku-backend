using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Engagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/visits")]
public sealed class SiteVisitsController(
    ISiteVisitService siteVisitService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SiteVisitModel>>
        RecordAsync(
            [FromBody] RecordSiteVisitRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await siteVisitService.RecordAsync(
                request.VisitorId,
                request.PagePath,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(result.Value);
    }
}

public sealed record RecordSiteVisitRequest(
    Guid VisitorId,
    string? PagePath);
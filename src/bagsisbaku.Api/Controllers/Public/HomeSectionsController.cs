using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/home-sections")]
public sealed class HomeSectionsController(
    IPublicHomeSectionQuery homeSectionQuery)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<PublicHomeSectionModel>>>
        GetAsync(
            [FromQuery(Name = "lang")] string? languageCode,
            CancellationToken cancellationToken)
    {
        _ = languageCode;

        var language =
            Request.ResolveLanguage();

        var sections =
            await homeSectionQuery.GetActiveAsync(
                language,
                cancellationToken);

        return Ok(sections);
    }
}
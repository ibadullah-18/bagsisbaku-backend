using bagsisbaku.Api.Extensions;
using bagsisbaku.Api.Models.Store;
using bagsisbaku.Application.Store;
using bagsisbaku.Contracts.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/store-settings")]
public sealed class StoreSettingsController(
    IStoreSettingsService storeSettingsService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PublicStoreSettingsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<PublicStoreSettingsResponse>>
        GetAsync(
            [FromQuery(Name = "lang")]
            string? languageCode,
            CancellationToken cancellationToken)
    {
        _ = languageCode;

        var language =
            Request.ResolveLanguage();

        var result =
            await storeSettingsService.GetPublicAsync(
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            StoreSettingsResponseMapper
                .ToPublicResponse(
                    result.Value));
    }
}
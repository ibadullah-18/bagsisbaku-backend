using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Engagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/catalog/products")]
public sealed class WhatsAppProductInquiriesController(
    IWhatsAppProductInquiryService inquiryService)
    : ControllerBase
{
    [HttpPost("{productId:guid}/whatsapp-inquiry")]
    public async Task<ActionResult<WhatsAppProductInquiryModel>>
        CreateAsync(
            Guid productId,
            [FromQuery(Name = "lang")] string? languageCode,
            CancellationToken cancellationToken)
    {
        _ = languageCode;

        var language =
            Request.ResolveLanguage();

        var result =
            await inquiryService.CreateAsync(
                productId,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(result.Value);
    }
}
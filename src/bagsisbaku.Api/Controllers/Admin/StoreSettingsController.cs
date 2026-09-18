using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Api.Models.Store;
using bagsisbaku.Contracts.Media;
using bagsisbaku.Application.Media;
using bagsisbaku.Api.Models.Media;
using bagsisbaku.Application.Security;
using bagsisbaku.Application.Store;
using bagsisbaku.Contracts.Store;
using bagsisbaku.Domain.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/store-settings")]
public sealed class StoreSettingsController(
    IStoreSettingsService storeSettingsService)
    : ControllerBase
{
    [HttpGet]
    [HasPermission(
        PermissionNames.StoreSettings.View)]
    [ProducesResponseType(
        typeof(StoreSettingsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreSettingsResponse>>
        GetAsync(
            CancellationToken cancellationToken)
    {
        var result =
            await storeSettingsService.GetAdminAsync(
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            StoreSettingsResponseMapper.ToResponse(
                result.Value));
    }

    [HttpPut]
    [HasPermission(
        PermissionNames.StoreSettings.Manage)]
    [ProducesResponseType(
        typeof(StoreSettingsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StoreSettingsResponse>>
        UpdateAsync(
            [FromBody]
            UpdateStoreSettingsRequest request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        var command =
            new UpdateStoreSettingsCommand(
                request.PrimaryPhone,
                request.WhatsAppPhone,
                request.Email,
                request.InstagramUrl,
                request.TikTokUrl,
                request.Address,
                request.WorkingHours,
                request.DeliveryInformation,
                request.ReturnPolicy,
                request.AboutText,
                request.Latitude,
                request.Longitude,
                request.MapUrl,
                request.RowVersion);

        var result =
            await storeSettingsService.UpdateAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            StoreSettingsResponseMapper.ToResponse(
                result.Value));
    }

    [HttpPut("translations")]
    [HasPermission(
        PermissionNames.StoreSettings.Manage)]
    [ProducesResponseType(
        typeof(StoreSettingsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StoreSettingsResponse>>
        UpsertTranslationAsync(
            [FromBody]
            UpsertStoreSettingsTranslationRequest request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        var language =
            ResolveTranslationLanguage(
                request.Language);

        var command =
            new UpsertStoreSettingsTranslationCommand(
                language,
                request.Address,
                request.WorkingHours,
                request.DeliveryInformation,
                request.ReturnPolicy,
                request.AboutText,
                request.RowVersion);

        var result =
            await storeSettingsService
                .UpsertTranslationAsync(
                    command,
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            StoreSettingsResponseMapper.ToResponse(
                result.Value));
    }

    [HttpPost("logo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 26_214_400)]
    [HasPermission(
        PermissionNames.StoreSettings.Manage)]
    [ProducesResponseType(
        typeof(UploadedImageResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UploadedImageResponse>>
        UploadLogoAsync(
            [FromForm]
            UploadImageForm request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        var file =
            request.File;

        await using var content =
            await CopyFileAsync(
                file,
                cancellationToken);

        var command =
            new ImageUploadCommand(
                content,
                file.FileName,
                file.ContentType,
                file.Length);

        var result =
            await storeSettingsService
                .UploadLogoAsync(
                    command,
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            new UploadedImageResponse(
                null,
                result.Value.Url,
                result.Value.Width,
                result.Value.Height,
                result.Value.Format,
                result.Value.Bytes,
                null,
                null));
    }

    [HttpDelete("logo")]
    [HasPermission(
        PermissionNames.StoreSettings.Manage)]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult>
        RemoveLogoAsync(
            CancellationToken cancellationToken)
    {
        var result =
            await storeSettingsService
                .RemoveLogoAsync(
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }

    private static async Task<MemoryStream>
        CopyFileAsync(
            IFormFile file,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            file);

        var content =
            file.Length > 0
                ? new MemoryStream(
                    checked((int)file.Length))
                : new MemoryStream();

        await file.CopyToAsync(
            content,
            cancellationToken);

        content.Position = 0;

        return content;
    }
    private static SupportedLanguage
        ResolveTranslationLanguage(
            string language)
    {
        return language
            .Trim()
            .ToLowerInvariant() switch
        {
            "az" or "aze" or "azərbaycan" =>
                SupportedLanguage.Azerbaijani,

            "ru" or "rus" or "russian" =>
                SupportedLanguage.Russian,

            "en" or "eng" or "english" =>
                SupportedLanguage.English,

            _ => (SupportedLanguage)0
        };
    }
}
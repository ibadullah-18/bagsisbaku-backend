using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Api.Models.Media;
using bagsisbaku.Application.Home;
using bagsisbaku.Application.Media;
using bagsisbaku.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/home-sections")]
public sealed class AdminHomeSectionPublicationController(
    IHomeSectionPublicationService publicationService)
    : ControllerBase
{
    [HttpPost("{sectionId:guid}/image")]
    [HasPermission(PermissionNames.Content.Manage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 26_214_400)]
    public async Task<ActionResult<HomeSectionPublicationModel>>
        UploadImageAsync(
            Guid sectionId,
            [FromForm] UploadImageForm request,
            CancellationToken cancellationToken)
    {
        var file = request.File;

        await using var content =
            new MemoryStream(
                capacity: checked((int)file.Length));

        await file.CopyToAsync(
            content,
            cancellationToken);

        content.Position = 0;

        var result =
            await publicationService.UploadImageAsync(
                sectionId,
                new ImageUploadCommand(
                    content,
                    file.FileName,
                    file.ContentType,
                    file.Length),
                cancellationToken);

        return result.IsFailure
            ? this.ToProblemResult(result.Error)
            : Ok(result.Value);
    }

    [HttpDelete("{sectionId:guid}/image")]
    [HasPermission(PermissionNames.Content.Manage)]
    public async Task<ActionResult<HomeSectionPublicationModel>>
        RemoveImageAsync(
            Guid sectionId,
            CancellationToken cancellationToken)
    {
        var result =
            await publicationService.RemoveImageAsync(
                sectionId,
                cancellationToken);

        return result.IsFailure
            ? this.ToProblemResult(result.Error)
            : Ok(result.Value);
    }

    [HttpPut("{sectionId:guid}/activation")]
    [HasPermission(PermissionNames.Content.Manage)]
    public async Task<ActionResult<HomeSectionPublicationModel>>
        SetActiveAsync(
            Guid sectionId,
            [FromBody] SetHomeSectionActivationRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await publicationService.SetActiveAsync(
                sectionId,
                request.IsActive,
                cancellationToken);

        return result.IsFailure
            ? this.ToProblemResult(result.Error)
            : Ok(result.Value);
    }
}

public sealed record SetHomeSectionActivationRequest(
    bool IsActive);
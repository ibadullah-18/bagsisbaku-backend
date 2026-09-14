using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Security;
using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Models.Media;
using bagsisbaku.Application.Media;
using bagsisbaku.Contracts.Media;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/media")]
public sealed class MediaController(
    IMediaAdministrationService mediaService)
    : ControllerBase
{
    [HttpPost("brands/{brandId:guid}/logo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 26_214_400)]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<ActionResult<UploadedImageResponse>>
        UploadBrandLogoAsync(
            Guid brandId,
            [FromForm] UploadImageForm request,
            CancellationToken cancellationToken)
    {
        var file = request.File;

        await using var content = await CopyAsync(
            file,
            cancellationToken);

        var result =
            await mediaService.UploadBrandLogoAsync(
                brandId,
                CreateCommand(file, content),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
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

    [HttpDelete("brands/{brandId:guid}/logo")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<IActionResult> RemoveBrandLogoAsync(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        var result =
            await mediaService.RemoveBrandLogoAsync(
                brandId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return NoContent();
    }

    [HttpPost("categories/{categoryId:guid}/icon")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 26_214_400)]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<ActionResult<UploadedImageResponse>>
        UploadCategoryIconAsync(
            Guid categoryId,
            [FromForm] UploadImageForm request,
            CancellationToken cancellationToken)
    {
        var file = request.File;

        await using var content = await CopyAsync(
            file,
            cancellationToken);

        var result =
            await mediaService.UploadCategoryIconAsync(
                categoryId,
                CreateCommand(file, content),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
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

    [HttpDelete("categories/{categoryId:guid}/icon")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<IActionResult> RemoveCategoryIconAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result =
            await mediaService.RemoveCategoryIconAsync(
                categoryId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return NoContent();
    }

    [HttpPost("products/{productId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 26_214_400)]
    [HasPermission(PermissionNames.Products.Update)]
    public async Task<ActionResult<UploadedImageResponse>>
        AddProductImageAsync(
            Guid productId,
            [FromForm] UploadImageForm request,
            CancellationToken cancellationToken)
    {
        var file = request.File;

        await using var content = await CopyAsync(
            file,
            cancellationToken);

        var result =
            await mediaService.AddProductImageAsync(
                productId,
                CreateCommand(file, content),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return Ok(
            new UploadedImageResponse(
                result.Value.ImageId,
                result.Value.StoredImage.Url,
                result.Value.StoredImage.Width,
                result.Value.StoredImage.Height,
                result.Value.StoredImage.Format,
                result.Value.StoredImage.Bytes,
                result.Value.IsPrimary,
                result.Value.SortOrder));
    }

    [HttpDelete(
        "products/{productId:guid}/images/{imageId:guid}")]
    [HasPermission(PermissionNames.Products.Update)]
    public async Task<IActionResult> RemoveProductImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result =
            await mediaService.RemoveProductImageAsync(
                productId,
                imageId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return NoContent();
    }

    [HttpPut(
        "products/{productId:guid}/images/{imageId:guid}/primary")]
    [HasPermission(PermissionNames.Products.Update)]
    public async Task<IActionResult> SetPrimaryImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result =
            await mediaService.SetPrimaryProductImageAsync(
                productId,
                imageId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return NoContent();
    }

    private static async Task<MemoryStream> CopyAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        var memoryStream = file.Length > 0
            ? new MemoryStream(
                capacity: checked((int)file.Length))
            : new MemoryStream();

        await file.CopyToAsync(
            memoryStream,
            cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    private static ImageUploadCommand CreateCommand(
        IFormFile file,
        Stream content)
    {
        return new ImageUploadCommand(
            content,
            file.FileName,
            file.ContentType,
            file.Length);
    }
}










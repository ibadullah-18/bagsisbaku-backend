using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Home;
using bagsisbaku.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/home-sections")]
public sealed class AdminHomeSectionEditorController(
    IAdminHomeSectionEditor editor)
    : ControllerBase
{
    [HttpPut("{sectionId:guid}")]
    [HasPermission(PermissionNames.Content.Manage)]
    public async Task<ActionResult<HomeSectionModel>>
        UpdateAsync(
            Guid sectionId,
            [FromBody] UpdateHomeSectionRequest request,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            request.Title.Length > 160 ||
            request.Subtitle?.Length > 500 ||
            request.TargetUrl?.Length > 2048 ||
            request.SortOrder < 0)
        {
            return BadRequest(
                "Başlıq, alt mətn, keçid və ya sıra nömrəsi düzgün deyil.");
        }

        if (request.Translations is not null)
        {
            if (request.Translations.Any(item =>
                    item.Language is not (2 or 3) ||
                    string.IsNullOrWhiteSpace(item.Title) ||
                    item.Title.Length > 160 ||
                    item.Subtitle?.Length > 500) ||
                request.Translations
                    .Select(item => item.Language)
                    .Distinct()
                    .Count() !=
                request.Translations.Count)
            {
                return BadRequest(
                    "RU/EN tərcümələrini və təkrarlanan dilləri yoxla.");
            }
        }

        var translations =
            request.Translations?
                .Select(item =>
                    new HomeSectionTranslationInput(
                        item.Language,
                        item.Title,
                        item.Subtitle))
                .ToArray();

        var result =
            await editor.UpdateAsync(
                sectionId,
                new UpdateHomeSectionCommand(
                    request.Title,
                    request.Subtitle,
                    request.TargetUrl,
                    request.SortOrder,
                    translations),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{sectionId:guid}")]
    [HasPermission(PermissionNames.Content.Manage)]
    public async Task<IActionResult> DeleteAsync(
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        var result =
            await editor.DeleteAsync(
                sectionId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }
}

public sealed record UpdateHomeSectionRequest(
    string Title,
    string? Subtitle,
    string? TargetUrl,
    int SortOrder,
    IReadOnlyList<HomeSectionTranslationRequest>? Translations);
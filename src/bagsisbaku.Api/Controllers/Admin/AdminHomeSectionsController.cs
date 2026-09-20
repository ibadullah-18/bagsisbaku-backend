using bagsisbaku.Api.Authorization;
using bagsisbaku.Application.Home;
using bagsisbaku.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/home-sections")]
public sealed class AdminHomeSectionsController(
    IAdminHomeSectionService homeSectionService)
    : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionNames.Content.View)]
    public async Task<ActionResult<IReadOnlyList<HomeSectionModel>>>
        GetAllAsync(CancellationToken cancellationToken)
    {
        var sections =
            await homeSectionService.GetAllAsync(
                cancellationToken);

        return Ok(sections);
    }

    [HttpPost]
    [HasPermission(PermissionNames.Content.Manage)]
    public async Task<ActionResult<HomeSectionModel>>
        CreateAsync(
            [FromBody] CreateHomeSectionRequest request,
            CancellationToken cancellationToken)
    {
        if (request.Type is not (1 or 2))
        {
            return BadRequest(
                "type yalnız 1 (Campaign) və ya 2 (Banner) ola bilər.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) ||
            request.Title.Length > 160 ||
            request.Subtitle?.Length > 500 ||
            request.TargetUrl?.Length > 2048 ||
            request.SortOrder < 0)
        {
            return BadRequest(
                "Başlıq, alt mətn, keçid və ya sıra nömrəsi düzgün deyil.");
        }

        var translations =
            request.Translations ??
            [];

        if (translations.Any(item =>
                item.Language is not (2 or 3) ||
                string.IsNullOrWhiteSpace(item.Title) ||
                item.Title.Length > 160 ||
                item.Subtitle?.Length > 500) ||
            translations
                .Select(item => item.Language)
                .Distinct()
                .Count() != translations.Count)
        {
            return BadRequest(
                "RU/EN tərcümələrini və təkrarlanan dilləri yoxla.");
        }

        var command =
            new CreateHomeSectionCommand(
                request.Type,
                request.Title,
                request.Subtitle,
                request.TargetUrl,
                request.SortOrder,
                translations
                    .Select(item =>
                        new HomeSectionTranslationInput(
                            item.Language,
                            item.Title,
                            item.Subtitle))
                    .ToArray());

        var section =
            await homeSectionService.CreateAsync(
                command,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            section);
    }
}

public sealed record CreateHomeSectionRequest(
    int Type,
    string Title,
    string? Subtitle,
    string? TargetUrl,
    int SortOrder,
    IReadOnlyList<HomeSectionTranslationRequest>? Translations);

public sealed record HomeSectionTranslationRequest(
    int Language,
    string Title,
    string? Subtitle);
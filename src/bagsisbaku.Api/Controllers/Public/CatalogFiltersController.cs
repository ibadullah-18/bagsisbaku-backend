using System.ComponentModel.DataAnnotations;
using bagsisbaku.Application.Catalog.Public;
using bagsisbaku.Contracts.Catalog;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Public;

[ApiController]
[Route("api/catalog/filters")]
public sealed class CatalogFiltersController(
    IPublicCatalogFilterQuery catalogFilterQuery)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PublicCatalogFilterResponse>>
        GetAsync(
            [FromQuery]
            [Range(1, 2)]
            int? productType,

            [FromQuery]
            string language = "az",

            CancellationToken cancellationToken = default)
    {
        if (!TryParseLanguage(
                language,
                out var supportedLanguage))
        {
            ModelState.AddModelError(
                nameof(language),
                "Dil az, ru və ya en olmalıdır.");

            return ValidationProblem(ModelState);
        }

        var selectedProductType = productType.HasValue
            ? (ProductType?)productType.Value
            : null;

        var result = await catalogFilterQuery.GetAsync(
            selectedProductType,
            supportedLanguage,
            cancellationToken);

        return Ok(
            new PublicCatalogFilterResponse(
                result.Language,
                result.ProductType,
                result.ProductCount,
                result.MinimumPrice,
                result.MaximumPrice,
                result.Brands
                    .Select(
                        brand =>
                            new PublicBrandFilterResponse(
                                brand.Id,
                                brand.Name,
                                brand.LogoUrl,
                                brand.ProductCount))
                    .ToArray(),
                result.Categories
                    .Select(
                        category =>
                            new PublicCategoryFilterResponse(
                                category.Id,
                                category.Name,
                                category.IconUrl,
                                category.ProductType,
                                category.ProductCount))
                    .ToArray(),
                result.Colors
                    .Select(
                        color =>
                            new PublicColorFilterResponse(
                                color.Id,
                                color.Name,
                                color.HexCode,
                                color.ProductCount))
                    .ToArray(),
                result.Sizes
                    .Select(
                        size =>
                            new PublicSizeFilterResponse(
                                size.Id,
                                size.Value,
                                size.ProductType,
                                size.SortOrder,
                                size.ProductCount))
                    .ToArray()));
    }

    private static bool TryParseLanguage(
        string? value,
        out SupportedLanguage language)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            language = SupportedLanguage.Azerbaijani;
            return true;
        }

        var code = value
            .Trim()
            .Split(
                ['-', '_'],
                StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.ToLowerInvariant();

        switch (code)
        {
            case "az":
                language =
                    SupportedLanguage.Azerbaijani;
                return true;

            case "ru":
                language =
                    SupportedLanguage.Russian;
                return true;

            case "en":
                language =
                    SupportedLanguage.English;
                return true;

            default:
                language =
                    SupportedLanguage.Azerbaijani;
                return false;
        }
    }
}

using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Application.Catalog.Administration.Models;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Catalog.Administration;

internal sealed class CatalogAdministrationService(
    IBrandRepository brandRepository,
    ICategoryRepository categoryRepository,
    ISizeRepository sizeRepository,
    IColorRepository colorRepository,
    ICatalogDefaultRepository catalogDefaultRepository,
    ITranslationRepository translationRepository,
    IUnitOfWork unitOfWork)
    : ICatalogAdministrationService
{
    public async Task<Result<Guid>> CreateBrandAsync(
        CreateBrandCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    "Brand adı boş ola bilməz."));
        }

        var nameExists =
            await brandRepository.NameExistsAsync(
                command.Name,
                cancellationToken: cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(
                CatalogErrors.BrandNameConflict);
        }

        try
        {
            var brand = Brand.Create(command.Name);

            await brandRepository.AddAsync(
                brand,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success(brand.Id);
        }
        catch (DomainException exception)
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    exception.Message));
        }
    }

    public async Task<Result<Guid>> CreateCategoryAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!TryGetProductType(
                command.ProductType,
                out var productType))
        {
            return Result.Failure<Guid>(
                CatalogErrors.InvalidProductType);
        }

        if (string.IsNullOrWhiteSpace(command.NameAz) ||
            string.IsNullOrWhiteSpace(command.NameRu) ||
            string.IsNullOrWhiteSpace(command.NameEn))
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    "Kateqoriya adı AZ, RU və EN dillərində yazılmalıdır."));
        }

        var nameExists =
            await categoryRepository.NameExistsAsync(
                command.NameAz,
                productType,
                cancellationToken: cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(
                CatalogErrors.CategoryNameConflict);
        }

        try
        {
            var category = Category.Create(
                command.NameAz,
                productType);

            var translations =
                new CategoryTranslation[]
                {
                    CategoryTranslation.Create(
                        category.Id,
                        SupportedLanguage.Russian,
                        command.NameRu),

                    CategoryTranslation.Create(
                        category.Id,
                        SupportedLanguage.English,
                        command.NameEn)
                };

            await categoryRepository.AddAsync(
                category,
                cancellationToken);

            await translationRepository
                .AddCategoryTranslationsAsync(
                    translations,
                    cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success(category.Id);
        }
        catch (DomainException exception)
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    exception.Message));
        }
    }

    public async Task<Result<Guid>> CreateSizeAsync(
        CreateSizeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!TryGetProductType(
                command.ProductType,
                out var productType))
        {
            return Result.Failure<Guid>(
                CatalogErrors.InvalidProductType);
        }

        if (string.IsNullOrWhiteSpace(command.ValueAz))
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    "Ölçünün Azərbaycan dilində dəyəri boş ola bilməz."));
        }

        var valueExists =
            await sizeRepository.ValueExistsAsync(
                command.ValueAz,
                productType,
                cancellationToken: cancellationToken);

        if (valueExists)
        {
            return Result.Failure<Guid>(
                CatalogErrors.SizeValueConflict);
        }

        try
        {
            var size = Size.Create(
                command.ValueAz,
                productType,
                command.SortOrder);

            var translations =
                CreateSizeTranslations(
                    size.Id,
                    command.ValueRu,
                    command.ValueEn);

            await sizeRepository.AddAsync(
                size,
                cancellationToken);

            await translationRepository
                .AddSizeTranslationsAsync(
                    translations,
                    cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success(size.Id);
        }
        catch (DomainException exception)
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    exception.Message));
        }
    }

    public async Task<Result<Guid>> CreateColorAsync(
        CreateColorCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.NameAz) ||
            string.IsNullOrWhiteSpace(command.NameRu) ||
            string.IsNullOrWhiteSpace(command.NameEn))
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    "Rəng adı AZ, RU və EN dillərində yazılmalıdır."));
        }

        var nameExists =
            await colorRepository.NameExistsAsync(
                command.NameAz,
                cancellationToken: cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(
                CatalogErrors.ColorNameConflict);
        }

        try
        {
            var color = Color.Create(
                command.NameAz,
                command.HexCode);

            var translations =
                new ColorTranslation[]
                {
                    ColorTranslation.Create(
                        color.Id,
                        SupportedLanguage.Russian,
                        command.NameRu),

                    ColorTranslation.Create(
                        color.Id,
                        SupportedLanguage.English,
                        command.NameEn)
                };

            await colorRepository.AddAsync(
                color,
                cancellationToken);

            await translationRepository
                .AddColorTranslationsAsync(
                    translations,
                    cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success(color.Id);
        }
        catch (DomainException exception)
        {
            return Result.Failure<Guid>(
                CatalogErrors.Invalid(
                    exception.Message));
        }
    }

    public async Task<Result> SetDefaultsAsync(
        SetCatalogDefaultCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!TryGetProductType(
                command.ProductType,
                out var productType))
        {
            return Result.Failure(
                CatalogErrors.InvalidProductType);
        }

        if (command.CategoryId == Guid.Empty ||
            command.SizeId == Guid.Empty)
        {
            return Result.Failure(
                CatalogErrors.Invalid(
                    "Default category və size seçilməlidir."));
        }

        var category =
            await categoryRepository.GetByIdAsync(
                command.CategoryId,
                cancellationToken);

        if (category is null || !category.IsActive)
        {
            return Result.Failure(
                CatalogErrors.CategoryNotFound);
        }

        if (category.ProductType != productType)
        {
            return Result.Failure(
                CatalogErrors.CategoryTypeMismatch);
        }

        var size = await sizeRepository.GetByIdAsync(
            command.SizeId,
            cancellationToken);

        if (size is null || !size.IsActive)
        {
            return Result.Failure(
                CatalogErrors.SizeNotFound);
        }

        if (size.ProductType != productType)
        {
            return Result.Failure(
                CatalogErrors.SizeTypeMismatch);
        }

        var catalogDefault =
            await catalogDefaultRepository
                .GetByProductTypeAsync(
                    productType,
                    cancellationToken);

        if (catalogDefault is null)
        {
            catalogDefault = CatalogDefault.Create(
                productType,
                command.CategoryId,
                command.SizeId);

            await catalogDefaultRepository.AddAsync(
                catalogDefault,
                cancellationToken);
        }
        else
        {
            catalogDefault.SetDefaults(
                command.CategoryId,
                command.SizeId);
        }

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<CatalogOptionsModel>>
        GetOptionsAsync(
            int productType,
            CancellationToken cancellationToken = default)
    {
        if (!TryGetProductType(
                productType,
                out var parsedProductType))
        {
            return Result.Failure<CatalogOptionsModel>(
                CatalogErrors.InvalidProductType);
        }

        var brands = await brandRepository.GetAllAsync(
            cancellationToken);

        var categories =
            await categoryRepository.GetByProductTypeAsync(
                parsedProductType,
                cancellationToken);

        var sizes =
            await sizeRepository.GetByProductTypeAsync(
                parsedProductType,
                cancellationToken);

        var colors = await colorRepository.GetAllAsync(
            cancellationToken);

        var catalogDefault =
            await catalogDefaultRepository
                .GetByProductTypeAsync(
                    parsedProductType,
                    cancellationToken);

        var model = new CatalogOptionsModel(
            parsedProductType,
            catalogDefault?.DefaultCategoryId,
            catalogDefault?.DefaultSizeId,
            brands
                .Where(brand => brand.IsActive)
                .Select(
                    brand => new BrandOptionModel(
                        brand.Id,
                        brand.Name,
                        brand.ImageUrl))
                .ToArray(),
            categories
                .Where(category => category.IsActive)
                .Select(
                    category => new CategoryOptionModel(
                        category.Id,
                        category.Name,
                        category.IconUrl))
                .ToArray(),
            sizes
                .Where(size => size.IsActive)
                .Select(
                    size => new SizeOptionModel(
                        size.Id,
                        size.Value,
                        size.SortOrder))
                .ToArray(),
            colors
                .Where(color => color.IsActive)
                .Select(
                    color => new ColorOptionModel(
                        color.Id,
                        color.Name,
                        color.HexCode))
                .ToArray());

        return Result.Success(model);
    }

    private static IReadOnlyCollection<SizeTranslation>
        CreateSizeTranslations(
            Guid sizeId,
            string? valueRu,
            string? valueEn)
    {
        var translations =
            new List<SizeTranslation>();

        if (!string.IsNullOrWhiteSpace(valueRu))
        {
            translations.Add(
                SizeTranslation.Create(
                    sizeId,
                    SupportedLanguage.Russian,
                    valueRu));
        }

        if (!string.IsNullOrWhiteSpace(valueEn))
        {
            translations.Add(
                SizeTranslation.Create(
                    sizeId,
                    SupportedLanguage.English,
                    valueEn));
        }

        return translations;
    }

    private static bool TryGetProductType(
        int value,
        out ProductType productType)
    {
        productType = (ProductType)value;

        return Enum.IsDefined(productType);
    }
}

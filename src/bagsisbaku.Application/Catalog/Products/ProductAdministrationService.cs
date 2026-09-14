using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Application.Catalog.Products.Models;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;
using FluentValidation;

namespace bagsisbaku.Application.Catalog.Products;

internal sealed class ProductAdministrationService(
    IProductRepository productRepository,
    IBrandRepository brandRepository,
    ICategoryRepository categoryRepository,
    ISizeRepository sizeRepository,
    IColorRepository colorRepository,
    ICatalogDefaultRepository catalogDefaultRepository,
    ITranslationRepository translationRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductCommand> validator)
    : IProductAdministrationService
{
    public async Task<Result<Guid>> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult =
            await validator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var description = string.Join(
                " ",
                validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .Distinct(StringComparer.Ordinal));

            return Result.Failure<Guid>(
                ProductErrors.Invalid(description));
        }

        var productType =
            (ProductType)command.ProductType;

        var codeExists =
            await productRepository.ProductCodeExistsAsync(
                command.ProductCode,
                cancellationToken: cancellationToken);

        if (codeExists)
        {
            return Result.Failure<Guid>(
                ProductErrors.ProductCodeConflict);
        }

        var brand = await brandRepository.GetByIdAsync(
            command.BrandId,
            cancellationToken);

        if (brand is null || !brand.IsActive)
        {
            return Result.Failure<Guid>(
                ProductErrors.BrandNotFound);
        }

        CatalogDefault? catalogDefault = null;

        var requiresDefault =
            !command.CategoryId.HasValue ||
            command.Variants.Any(
                variant => !variant.SizeId.HasValue);

        if (requiresDefault)
        {
            catalogDefault =
                await catalogDefaultRepository
                    .GetByProductTypeAsync(
                        productType,
                        cancellationToken);

            if (catalogDefault is null)
            {
                return Result.Failure<Guid>(
                    ProductErrors
                        .DefaultConfigurationMissing(
                            productType));
            }
        }

        var categoryId =
            command.CategoryId ??
            catalogDefault!.DefaultCategoryId;

        var category =
            await categoryRepository.GetByIdAsync(
                categoryId,
                cancellationToken);

        if (category is null || !category.IsActive)
        {
            return Result.Failure<Guid>(
                ProductErrors.CategoryNotFound);
        }

        if (category.ProductType != productType)
        {
            return Result.Failure<Guid>(
                ProductErrors.CategoryTypeMismatch);
        }

        try
        {
            var product = Product.Create(
                command.NameAz,
                command.DescriptionAz,
                command.ProductCode,
                command.Model,
                command.Price,
                command.DiscountPrice,
                productType,
                categoryId,
                command.BrandId,
                command.IsFeatured);

            foreach (var variantCommand in command.Variants)
            {
                var sizeId =
                    variantCommand.SizeId ??
                    catalogDefault!.DefaultSizeId;

                var size = await sizeRepository.GetByIdAsync(
                    sizeId,
                    cancellationToken);

                if (size is null || !size.IsActive)
                {
                    return Result.Failure<Guid>(
                        ProductErrors.SizeNotFound);
                }

                if (size.ProductType != productType)
                {
                    return Result.Failure<Guid>(
                        ProductErrors.SizeTypeMismatch);
                }

                var color =
                    await colorRepository.GetByIdAsync(
                        variantCommand.ColorId,
                        cancellationToken);

                if (color is null || !color.IsActive)
                {
                    return Result.Failure<Guid>(
                        ProductErrors.ColorNotFound);
                }

                product.AddVariant(
                    sizeId,
                    variantCommand.ColorId,
                    variantCommand.StockCount);
            }

            var translations =
                new ProductTranslation[]
                {
                    ProductTranslation.Create(
                        product.Id,
                        SupportedLanguage.Russian,
                        command.NameRu,
                        command.DescriptionRu),

                    ProductTranslation.Create(
                        product.Id,
                        SupportedLanguage.English,
                        command.NameEn,
                        command.DescriptionEn)
                };

            await productRepository.AddAsync(
                product,
                cancellationToken);

            await translationRepository
                .AddProductTranslationsAsync(
                    translations,
                    cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success(product.Id);
        }
        catch (DomainException exception)
        {
            return Result.Failure<Guid>(
                ProductErrors.Invalid(
                    exception.Message));
        }
    }

    public async Task<Result<ProductDetailsModel>> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<ProductDetailsModel>(
                ProductErrors.NotFound);
        }

        var product =
            await productRepository
                .GetWithDetailsByIdAsync(
                    productId,
                    cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDetailsModel>(
                ProductErrors.NotFound);
        }

        var model = new ProductDetailsModel(
            product.Id,
            product.Name,
            product.Description,
            product.ProductCode,
            product.Model,
            product.Price,
            product.DiscountPrice,
            product.IsDiscounted,
            product.IsFeatured,
            product.IsActive,
            product.ViewCount,
            product.ProductType,
            product.CategoryId,
            product.BrandId,
            product.CreatedAtUtc,
            product.UpdatedAtUtc,
            product.Images
                .OrderBy(image => image.SortOrder)
                .Select(
                    image => new ProductImageModel(
                        image.Id,
                        image.ImageUrl,
                        image.SortOrder,
                        image.IsPrimary))
                .ToArray(),
            product.Variants
                .OrderBy(variant => variant.Id)
                .Select(
                    variant => new ProductVariantModel(
                        variant.Id,
                        variant.SizeId,
                        variant.ColorId,
                        variant.StockCount,
                        variant.IsActive))
                .ToArray());

        return Result.Success(model);
    }
}

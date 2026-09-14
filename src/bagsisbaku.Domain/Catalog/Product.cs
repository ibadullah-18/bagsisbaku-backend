using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class Product : AuditableEntity
{
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductVariant> _variants = [];

    private Product()
    {
    }

    private Product(
        Guid id,
        string name,
        string? description,
        string productCode,
        string? model,
        decimal price,
        decimal? discountPrice,
        ProductType productType,
        Guid categoryId,
        Guid brandId,
        bool isFeatured)
        : base(id)
    {
        ProductType = DomainGuard.DefinedEnum(
            productType,
            nameof(ProductType));

        CategoryId = DomainGuard.NotEmpty(
            categoryId,
            nameof(CategoryId));

        BrandId = DomainGuard.NotEmpty(
            brandId,
            nameof(BrandId));

        UpdateDetails(
            name,
            description,
            productCode,
            model);

        UpdatePricing(
            price,
            discountPrice);

        IsFeatured = isFeatured;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string? Model { get; private set; }

    public decimal Price { get; private set; }

    public decimal? DiscountPrice { get; private set; }

    public bool IsDiscounted => DiscountPrice.HasValue;

    public bool IsFeatured { get; private set; }

    public bool IsActive { get; private set; }

    public int ViewCount { get; private set; }

    public ProductType ProductType { get; private set; }

    public Guid CategoryId { get; private set; }

    public Guid BrandId { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images;

    public IReadOnlyCollection<ProductVariant> Variants => _variants;

    public static Product Create(
        string name,
        string? description,
        string productCode,
        string? model,
        decimal price,
        decimal? discountPrice,
        ProductType productType,
        Guid categoryId,
        Guid brandId,
        bool isFeatured = false)
    {
        return new Product(
            Guid.NewGuid(),
            name,
            description,
            productCode,
            model,
            price,
            discountPrice,
            productType,
            categoryId,
            brandId,
            isFeatured);
    }

    public void UpdateDetails(
        string name,
        string? description,
        string productCode,
        string? model)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            200);

        Description = DomainGuard.Optional(
            description,
            nameof(Description),
            4000);

        ProductCode = DomainGuard.Required(
            productCode,
            nameof(ProductCode),
            80);

        Model = DomainGuard.Optional(
            model,
            nameof(Model),
            120);
    }

    public void UpdatePricing(
        decimal price,
        decimal? discountPrice)
    {
        var validPrice = DomainGuard.Positive(
            price,
            nameof(Price));

        decimal? validDiscountPrice = null;

        if (discountPrice.HasValue)
        {
            validDiscountPrice = DomainGuard.Positive(
                discountPrice.Value,
                nameof(DiscountPrice));

            if (validDiscountPrice.Value >= validPrice)
            {
                throw new DomainException(
                    "Endirimli qiymət əsas qiymətdən aşağı olmalıdır.");
            }
        }

        Price = validPrice;
        DiscountPrice = validDiscountPrice;
    }

    public void ChangeCategory(Guid categoryId)
    {
        CategoryId = DomainGuard.NotEmpty(
            categoryId,
            nameof(CategoryId));
    }

    public void ChangeBrand(Guid brandId)
    {
        BrandId = DomainGuard.NotEmpty(
            brandId,
            nameof(BrandId));
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void RegisterView()
    {
        ViewCount = checked(ViewCount + 1);
    }

    public ProductVariant AddVariant(
        Guid sizeId,
        Guid colorId,
        int stockCount)
    {
        var validSizeId = DomainGuard.NotEmpty(
            sizeId,
            nameof(sizeId));

        var validColorId = DomainGuard.NotEmpty(
            colorId,
            nameof(colorId));

        var variantAlreadyExists = _variants.Any(
            variant =>
                variant.SizeId == validSizeId &&
                variant.ColorId == validColorId);

        if (variantAlreadyExists)
        {
            throw new DomainException(
                "Bu ölçü və rəng kombinasiyası artıq mövcuddur.");
        }

        var variant = ProductVariant.Create(
            Id,
            validSizeId,
            validColorId,
            stockCount);

        _variants.Add(variant);

        return variant;
    }

    public void DeactivateVariant(Guid variantId)
    {
        var variant = FindVariant(variantId);
        variant.Deactivate();
    }

    public void ActivateVariant(Guid variantId)
    {
        var variant = FindVariant(variantId);
        variant.Activate();
    }

    public ProductImage AddImage(
        string imageUrl,
        string imagePublicId)
    {
        var image = ProductImage.Create(
            Id,
            imageUrl,
            imagePublicId,
            _images.Count,
            isPrimary: _images.Count == 0);

        _images.Add(image);

        return image;
    }

    public void SetPrimaryImage(Guid imageId)
    {
        DomainGuard.NotEmpty(
            imageId,
            nameof(imageId));

        var selectedImage = _images.SingleOrDefault(
            image => image.Id == imageId);

        if (selectedImage is null)
        {
            throw new DomainException(
                "Məhsul şəkli tapılmadı.");
        }

        foreach (var image in _images)
        {
            image.SetPrimary(image.Id == selectedImage.Id);
        }
    }

    public ProductImage RemoveImage(Guid imageId)
    {
        DomainGuard.NotEmpty(
            imageId,
            nameof(imageId));

        var image = _images.SingleOrDefault(
            currentImage => currentImage.Id == imageId);

        if (image is null)
        {
            throw new DomainException(
                "Məhsul şəkli tapılmadı.");
        }

        var removedImageWasPrimary = image.IsPrimary;

        _images.Remove(image);

        RecalculateImageOrder();

        if (removedImageWasPrimary && _images.Count > 0)
        {
            var firstImage = _images
                .OrderBy(currentImage => currentImage.SortOrder)
                .First();

            SetPrimaryImage(firstImage.Id);
        }

        return image;
    }

    public void ReorderImages(
        IReadOnlyList<Guid> orderedImageIds)
    {
        ArgumentNullException.ThrowIfNull(orderedImageIds);

        var containsDuplicateIds =
            orderedImageIds.Distinct().Count() !=
            orderedImageIds.Count;

        if (containsDuplicateIds)
        {
            throw new DomainException(
                "Şəkil sıralamasında təkrarlanan Id var.");
        }

        if (orderedImageIds.Count != _images.Count)
        {
            throw new DomainException(
                "Bütün məhsul şəkilləri sıralamaya daxil edilməlidir.");
        }

        for (var index = 0; index < orderedImageIds.Count; index++)
        {
            var imageId = orderedImageIds[index];

            var image = _images.SingleOrDefault(
                currentImage => currentImage.Id == imageId);

            if (image is null)
            {
                throw new DomainException(
                    "Sıralamadakı şəkillərdən biri məhsula aid deyil.");
            }

            image.SetSortOrder(index);
        }
    }

    private ProductVariant FindVariant(Guid variantId)
    {
        DomainGuard.NotEmpty(
            variantId,
            nameof(variantId));

        var variant = _variants.SingleOrDefault(
            currentVariant => currentVariant.Id == variantId);

        if (variant is null)
        {
            throw new DomainException(
                "Məhsul variantı tapılmadı.");
        }

        return variant;
    }

    private void RecalculateImageOrder()
    {
        var orderedImages = _images
            .OrderBy(image => image.SortOrder)
            .ToList();

        for (var index = 0; index < orderedImages.Count; index++)
        {
            orderedImages[index].SetSortOrder(index);
        }
    }
}

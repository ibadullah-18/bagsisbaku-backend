using bagsisbaku.Domain.Catalog;
using FluentValidation;

namespace bagsisbaku.Application.Catalog.Products;

public sealed class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.NameAz)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.NameRu)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.NameEn)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.DescriptionAz)
            .MaximumLength(4000);

        RuleFor(command => command.DescriptionRu)
            .MaximumLength(4000);

        RuleFor(command => command.DescriptionEn)
            .MaximumLength(4000);

        RuleFor(command => command.ProductCode)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(command => command.Model)
            .MaximumLength(120);

        RuleFor(command => command.Price)
            .GreaterThan(0);

        RuleFor(command => command.DiscountPrice)
            .GreaterThan(0)
            .When(command => command.DiscountPrice.HasValue);

        RuleFor(command => command)
            .Must(
                command =>
                    !command.DiscountPrice.HasValue ||
                    command.DiscountPrice.Value <
                    command.Price)
            .WithMessage(
                "Endirimli qiymət əsas qiymətdən aşağı olmalıdır.");

        RuleFor(command => command.ProductType)
            .Must(
                value =>
                    Enum.IsDefined(
                        typeof(ProductType),
                        value))
            .WithMessage(
                "Məhsul tipi yalnız Shoe və ya Bag ola bilər.");

        RuleFor(command => command.CategoryId)
            .Must(
                categoryId =>
                    !categoryId.HasValue ||
                    categoryId.Value != Guid.Empty)
            .WithMessage(
                "CategoryId düzgün deyil.");

        RuleFor(command => command.BrandId)
            .NotEmpty();

        RuleFor(command => command.Variants)
            .NotNull()
            .NotEmpty()
            .Must(HaveUniqueVariants)
            .WithMessage(
                "Eyni ölçü və rəng kombinasiyası təkrarlana bilməz.");

        RuleForEach(command => command.Variants)
            .SetValidator(
                new CreateProductVariantCommandValidator());
    }

    private static bool HaveUniqueVariants(
        IReadOnlyList<CreateProductVariantCommand> variants)
    {
        if (variants is null)
        {
            return false;
        }

        return variants
            .GroupBy(
                variant => new
                {
                    variant.SizeId,
                    variant.ColorId
                })
            .All(group => group.Count() == 1);
    }
}

public sealed class CreateProductVariantCommandValidator
    : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(command => command.SizeId)
            .Must(
                sizeId =>
                    !sizeId.HasValue ||
                    sizeId.Value != Guid.Empty)
            .WithMessage(
                "SizeId düzgün deyil.");

        RuleFor(command => command.ColorId)
            .NotEmpty();

        RuleFor(command => command.StockCount)
            .GreaterThanOrEqualTo(0);
    }
}

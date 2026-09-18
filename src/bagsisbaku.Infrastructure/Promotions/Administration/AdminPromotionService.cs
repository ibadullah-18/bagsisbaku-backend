using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Promotions.Administration;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Promotions;
using bagsisbaku.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Promotions.Administration;

internal sealed class AdminPromotionService(
    ApplicationDbContext dbContext,
    IClock clock,
    IValidator<AdminPromoCodeFilter> filterValidator,
    IValidator<CreatePromoCodeCommand> createValidator,
    IValidator<UpdatePromoCodeCommand> updateValidator)
    : IAdminPromotionService
{
    public async Task<Result<AdminPromoCodePageModel>>
        GetAllAsync(
            AdminPromoCodeFilter filter,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            filter);

        var validationResult =
            await filterValidator.ValidateAsync(
                filter,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<
                AdminPromoCodePageModel>(
                    ToValidationError(
                        validationResult));
        }

        var query =
            dbContext.PromoCodes
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(
            filter.Search))
        {
            var search =
                filter.Search.Trim();

            query =
                query.Where(
                    promoCode =>
                        promoCode.Name.Contains(
                            search) ||
                        promoCode.Code.Contains(
                            search));
        }

        if (filter.IsActive.HasValue)
        {
            query =
                query.Where(
                    promoCode =>
                        promoCode.IsActive ==
                        filter.IsActive.Value);
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var entities =
            await query
                .OrderByDescending(
                    promoCode =>
                        promoCode.CreatedAtUtc)
                .ThenBy(
                    promoCode =>
                        promoCode.Code)
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(
                    filter.PageSize)
                .ToArrayAsync(
                    cancellationToken);

        var items =
            entities
                .Select(ToModel)
                .ToArray();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)filter.PageSize);

        return Result.Success(
            new AdminPromoCodePageModel(
                filter.Page,
                filter.PageSize,
                totalCount,
                totalPages,
                filter.Page > 1,
                filter.Page < totalPages,
                items));
    }

    public async Task<Result<AdminPromoCodeModel>>
        GetByIdAsync(
            Guid promoCodeId,
            CancellationToken cancellationToken = default)
    {
        if (promoCodeId == Guid.Empty)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .PromoCodeNotFound);
        }

        var promoCode =
            await dbContext.PromoCodes
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    entity =>
                        entity.Id ==
                        promoCodeId,
                    cancellationToken);

        if (promoCode is null)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .PromoCodeNotFound);
        }

        return Result.Success(
            ToModel(promoCode));
    }

    public async Task<Result<AdminPromoCodeModel>>
        CreateAsync(
            CreatePromoCodeCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        var validationResult =
            await createValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    ToValidationError(
                        validationResult));
        }

        var normalizedCode =
            NormalizeCode(
                command.Code);

        var codeExists =
            await dbContext.PromoCodes
                .AsNoTracking()
                .AnyAsync(
                    promoCode =>
                        promoCode.Code ==
                        normalizedCode,
                    cancellationToken);

        if (codeExists)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .CodeAlreadyExists);
        }

        PromoCode promoCode;

        try
        {
            promoCode =
                PromoCode.Create(
                    command.Name,
                    command.Code,
                    (PromotionDiscountType)
                        command.DiscountType,
                    command.DiscountValue,
                    command.MinimumOrderAmount,
                    command.MaximumDiscountAmount,
                    command.UsageLimit,
                    command.PerCustomerUsageLimit,
                    command.StartsAtUtc,
                    command.EndsAtUtc);
        }
        catch (DomainException exception)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors.Validation(
                        exception.Message));
        }

        await dbContext.PromoCodes.AddAsync(
            promoCode,
            cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .CodeAlreadyExists);
        }

        return Result.Success(
            ToModel(promoCode));
    }

    public async Task<Result<AdminPromoCodeModel>>
        UpdateAsync(
            Guid promoCodeId,
            UpdatePromoCodeCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        if (promoCodeId == Guid.Empty)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .PromoCodeNotFound);
        }

        var validationResult =
            await updateValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    ToValidationError(
                        validationResult));
        }

        var promoCode =
            await dbContext.PromoCodes
                .SingleOrDefaultAsync(
                    entity =>
                        entity.Id ==
                        promoCodeId,
                    cancellationToken);

        if (promoCode is null)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .PromoCodeNotFound);
        }

        var normalizedCode =
            NormalizeCode(
                command.Code);

        var duplicateCode =
            await dbContext.PromoCodes
                .AsNoTracking()
                .AnyAsync(
                    entity =>
                        entity.Id !=
                            promoCodeId &&
                        entity.Code ==
                            normalizedCode,
                    cancellationToken);

        if (duplicateCode)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .CodeAlreadyExists);
        }

        try
        {
            promoCode.Update(
                command.Name,
                command.Code,
                (PromotionDiscountType)
                    command.DiscountType,
                command.DiscountValue,
                command.MinimumOrderAmount,
                command.MaximumDiscountAmount,
                command.UsageLimit,
                command.PerCustomerUsageLimit,
                command.StartsAtUtc,
                command.EndsAtUtc);
        }
        catch (DomainException exception)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors.Validation(
                        exception.Message));
        }

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .ConcurrencyConflict);
        }
        catch (DbUpdateException)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .CodeAlreadyExists);
        }

        return Result.Success(
            ToModel(promoCode));
    }

    public async Task<Result<AdminPromoCodeModel>>
        SetStatusAsync(
            Guid promoCodeId,
            SetPromoCodeStatusCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        if (promoCodeId == Guid.Empty)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .PromoCodeNotFound);
        }

        var promoCode =
            await dbContext.PromoCodes
                .SingleOrDefaultAsync(
                    entity =>
                        entity.Id ==
                        promoCodeId,
                    cancellationToken);

        if (promoCode is null)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .PromoCodeNotFound);
        }

        if (command.IsActive)
        {
            promoCode.Activate();
        }
        else
        {
            promoCode.Deactivate();
        }

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<
                AdminPromoCodeModel>(
                    AdminPromotionErrors
                        .ConcurrencyConflict);
        }

        return Result.Success(
            ToModel(promoCode));
    }

    private AdminPromoCodeModel ToModel(
        PromoCode promoCode)
    {
        int? remainingUsageCount =
            promoCode.UsageLimit.HasValue
                ? Math.Max(
                    promoCode.UsageLimit.Value -
                    promoCode.UsageCount,
                    0)
                : null;

        var discountTypeName =
            promoCode.DiscountType switch
            {
                PromotionDiscountType.Percentage =>
                    "Faiz",

                PromotionDiscountType.FixedAmount =>
                    "Sabit məbləğ",

                _ =>
                    "Naməlum"
            };

        return new AdminPromoCodeModel(
            promoCode.Id,
            promoCode.Name,
            promoCode.Code,
            (int)promoCode.DiscountType,
            discountTypeName,
            promoCode.DiscountValue,
            promoCode.MinimumOrderAmount,
            promoCode.MaximumDiscountAmount,
            promoCode.UsageLimit,
            promoCode.UsageCount,
            remainingUsageCount,
            promoCode.PerCustomerUsageLimit,
            promoCode.StartsAtUtc,
            promoCode.EndsAtUtc,
            promoCode.IsActive,
            promoCode.IsAvailableAt(
                clock.UtcNow),
            promoCode.CreatedAtUtc,
            promoCode.UpdatedAtUtc);
    }

    private static string NormalizeCode(
        string code)
    {
        return code
            .Trim()
            .ToUpperInvariant();
    }

    private static Error ToValidationError(
        ValidationResult validationResult)
    {
        var description =
            string.Join(
                " ",
                validationResult.Errors
                    .Select(error =>
                        error.ErrorMessage)
                    .Distinct(
                        StringComparer.Ordinal));

        return AdminPromotionErrors.Validation(
            description);
    }
}
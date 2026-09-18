using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Promotions.Administration;

public interface IAdminPromotionService
{
    Task<Result<AdminPromoCodePageModel>> GetAllAsync(
        AdminPromoCodeFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<AdminPromoCodeModel>> GetByIdAsync(
        Guid promoCodeId,
        CancellationToken cancellationToken = default);

    Task<Result<AdminPromoCodeModel>> CreateAsync(
        CreatePromoCodeCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AdminPromoCodeModel>> UpdateAsync(
        Guid promoCodeId,
        UpdatePromoCodeCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AdminPromoCodeModel>> SetStatusAsync(
        Guid promoCodeId,
        SetPromoCodeStatusCommand command,
        CancellationToken cancellationToken = default);
}
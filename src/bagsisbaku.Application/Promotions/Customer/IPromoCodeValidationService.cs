using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Promotions.Customer;

public interface IPromoCodeValidationService
{
    Task<Result<PromoCodePreviewModel>> ValidateAsync(
        ValidatePromoCodeCommand command,
        CancellationToken cancellationToken = default);
}
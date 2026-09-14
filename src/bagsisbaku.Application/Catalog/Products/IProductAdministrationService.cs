using bagsisbaku.Application.Catalog.Products.Models;
using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Catalog.Products;

public interface IProductAdministrationService
{
    Task<Result<Guid>> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<ProductDetailsModel>> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}

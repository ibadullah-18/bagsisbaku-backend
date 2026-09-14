using bagsisbaku.Application.Catalog.Administration.Models;
using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Catalog.Administration;

public interface ICatalogAdministrationService
{
    Task<Result<Guid>> CreateBrandAsync(
        CreateBrandCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateCategoryAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateSizeAsync(
        CreateSizeCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateColorAsync(
        CreateColorCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> SetDefaultsAsync(
        SetCatalogDefaultCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<CatalogOptionsModel>> GetOptionsAsync(
        int productType,
        CancellationToken cancellationToken = default);
}

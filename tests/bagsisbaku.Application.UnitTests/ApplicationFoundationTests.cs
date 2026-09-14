using bagsisbaku.Application.Common.Pagination;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class ApplicationFoundationTests
{
    [Fact]
    public void SuccessfulResultShouldExposeValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailedResultShouldNotExposeValue()
    {
        var error = Error.NotFound(
            "products.not_found",
            "Məhsul tapılmadı.");

        var result = Result.Failure<int>(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);

        Assert.Throws<InvalidOperationException>(
            () => result.Value);
    }

    [Fact]
    public void PageRequestShouldCalculateSkip()
    {
        var request = new PageRequest(
            pageNumber: 3,
            pageSize: 20);

        Assert.Equal(40, request.Skip);
    }

    [Fact]
    public void PagedResultShouldCalculateMetadata()
    {
        var result = PagedResult<int>.Create(
            [1, 2, 3],
            pageNumber: 2,
            pageSize: 3,
            totalCount: 10);

        Assert.Equal(4, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void ProductAdminShouldNotAccessOrders()
    {
        IReadOnlySet<string> permissions =
            new HashSet<string>(
                StringComparer.Ordinal)
            {
                PermissionNames.Dashboard.View,
                PermissionNames.Products.View,
                PermissionNames.Products.Create,
                PermissionNames.Products.Update
            };

        var canViewProducts =
            PermissionEvaluator.HasPermission(
                isSuperAdmin: false,
                permissions,
                PermissionNames.Products.View);

        var canViewOrders =
            PermissionEvaluator.HasPermission(
                isSuperAdmin: false,
                permissions,
                PermissionNames.Orders.View);

        Assert.True(canViewProducts);
        Assert.False(canViewOrders);
    }

    [Fact]
    public void SuperAdminShouldAccessEveryKnownPermission()
    {
        IReadOnlySet<string> emptyPermissions =
            new HashSet<string>(
                StringComparer.Ordinal);

        foreach (var permission in PermissionNames.All)
        {
            var hasPermission =
                PermissionEvaluator.HasPermission(
                    isSuperAdmin: true,
                    emptyPermissions,
                    permission);

            Assert.True(hasPermission);
        }
    }

    [Fact]
    public void UnknownPermissionShouldBeRejectedForSuperAdmin()
    {
        IReadOnlySet<string> emptyPermissions =
            new HashSet<string>(
                StringComparer.Ordinal);

        var hasPermission =
            PermissionEvaluator.HasPermission(
                isSuperAdmin: true,
                emptyPermissions,
                "unknown.permission");

        Assert.False(hasPermission);
    }

    [Fact]
    public void PermissionNamesShouldBeUniqueAndLowercase()
    {
        var permissions = PermissionNames.All.ToArray();

        Assert.Equal(
            permissions.Length,
            permissions.Distinct(
                StringComparer.Ordinal).Count());

        foreach (var permission in permissions)
        {
            Assert.Equal(
                permission.ToLowerInvariant(),
                permission);
        }
    }
}

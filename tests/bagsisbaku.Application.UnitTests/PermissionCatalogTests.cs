using bagsisbaku.Application.Security;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class PermissionCatalogTests
{
    [Fact]
    public void PermissionCatalogShouldContainEveryPermission()
    {
        var catalogPermissions =
            PermissionCatalog.All
                .Select(definition => definition.Name)
                .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            PermissionNames.All.Count,
            catalogPermissions.Count);

        Assert.True(
            PermissionNames.All.SetEquals(
                catalogPermissions));
    }

    [Fact]
    public void PermissionCatalogShouldNotContainDuplicates()
    {
        var names =
            PermissionCatalog.All
                .Select(definition => definition.Name)
                .ToArray();

        Assert.Equal(
            names.Length,
            names.Distinct(
                StringComparer.Ordinal).Count());
    }
}

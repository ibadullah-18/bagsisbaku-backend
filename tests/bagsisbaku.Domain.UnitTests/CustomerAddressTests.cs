using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Customers;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class CustomerAddressTests
{
    [Fact]
    public void CreateShouldProduceValidAddress()
    {
        var userId = Guid.NewGuid();

        var address = CustomerAddress.Create(
            userId,
            "Ev",
            "Ibadulla Huseynzade",
            "+994501112233",
            "Bakı",
            "Yasamal",
            "Mətbuat prospekti 15, mənzil 20",
            "AZ1000",
            "Çatanda zəng edin",
            40.3775m,
            49.8096m,
            true);

        Assert.NotEqual(
            Guid.Empty,
            address.Id);

        Assert.Equal(
            userId,
            address.UserId);

        Assert.Equal(
            "Ev",
            address.Title);

        Assert.True(
            address.IsDefault);
    }

    [Fact]
    public void CreateShouldRejectIncompleteCoordinates()
    {
        Assert.Throws<DomainException>(
            () => CustomerAddress.Create(
                Guid.NewGuid(),
                "Ev",
                "Ibadulla Huseynzade",
                "+994501112233",
                "Bakı",
                "Yasamal",
                "Mətbuat prospekti 15",
                null,
                null,
                40.3775m,
                null));
    }

    [Fact]
    public void DefaultStatusShouldBeChangeable()
    {
        var address = CustomerAddress.Create(
            Guid.NewGuid(),
            "İş",
            "Ibadulla Huseynzade",
            "+994501112233",
            "Bakı",
            null,
            "Nizami küçəsi 10",
            null,
            null);

        Assert.False(
            address.IsDefault);

        address.SetAsDefault();

        Assert.True(
            address.IsDefault);

        address.RemoveDefault();

        Assert.False(
            address.IsDefault);
    }
}
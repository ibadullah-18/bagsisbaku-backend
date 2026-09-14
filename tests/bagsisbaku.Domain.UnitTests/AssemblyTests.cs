using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class AssemblyTests
{
    [Fact]
    public void Domain_assembly_should_have_expected_name()
    {
        var assemblyName =
            bagsisbaku.Domain.AssemblyReference
                .Assembly
                .GetName()
                .Name;

        Assert.Equal("bagsisbaku.Domain", assemblyName);
    }
}

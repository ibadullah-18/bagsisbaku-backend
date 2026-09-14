using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class AssemblyTests
{
    [Fact]
    public void ApplicationAssemblyShouldHaveExpectedName()
    {
        var assemblyName =
            bagsisbaku.Application.AssemblyReference
                .Assembly
                .GetName()
                .Name;

        Assert.Equal("bagsisbaku.Application", assemblyName);
    }
}


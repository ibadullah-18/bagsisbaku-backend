using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed class ApiAssemblyTests
{
    [Fact]
    public void ApiAssemblyShouldBeLoadable()
    {
        var assemblyName =
            bagsisbaku.Api.AssemblyReference
                .Assembly
                .GetName()
                .Name;

        Assert.Equal("bagsisbaku.Api", assemblyName);
    }
}


using NetArchTest.Rules;
using Xunit;

namespace bagsisbaku.ArchitectureTests;

public sealed class DependencyTests
{
    [Theory]
    [InlineData("bagsisbaku.Application")]
    [InlineData("bagsisbaku.Infrastructure")]
    [InlineData("bagsisbaku.Api")]
    [InlineData("bagsisbaku.Worker")]
    public void DomainShouldNotDependOnOuterLayers(
        string forbiddenLayer)
    {
        var result = Types
            .InAssembly(
                bagsisbaku.Domain.AssemblyReference.Assembly)
            .ShouldNot()
            .HaveDependencyOn(forbiddenLayer)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Domain must not depend on {forbiddenLayer}.");
    }
}


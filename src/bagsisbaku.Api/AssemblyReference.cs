using System.Reflection;

namespace bagsisbaku.Api;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } =
        typeof(AssemblyReference).Assembly;
}

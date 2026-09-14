using System.Reflection;

namespace bagsisbaku.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } =
        typeof(AssemblyReference).Assembly;
}

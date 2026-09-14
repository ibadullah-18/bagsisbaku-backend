using System.Reflection;

namespace bagsisbaku.Contracts;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } =
        typeof(AssemblyReference).Assembly;
}

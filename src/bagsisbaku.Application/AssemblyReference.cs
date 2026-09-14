using System.Reflection;

namespace bagsisbaku.Application;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } =
        typeof(AssemblyReference).Assembly;
}

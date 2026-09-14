using System.Reflection;

namespace bagsisbaku.Domain;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } =
        typeof(AssemblyReference).Assembly;
}

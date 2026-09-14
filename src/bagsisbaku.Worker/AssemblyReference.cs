using System.Reflection;

namespace bagsisbaku.Worker;

public static class AssemblyReference
{
    public static Assembly Assembly { get; } =
        typeof(AssemblyReference).Assembly;
}

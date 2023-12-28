using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PWP.Maui.Utils;

public class Guard
{
    public static T ThrowIfNull<T>([NotNull] T arg, [CallerArgumentExpression("arg")] string param = "") where T : class?
    {
        if (arg is null)
            throw new ArgumentNullException(string.IsNullOrEmpty(param) ? typeof(T).Name : param);

        return arg;
    }

    public static string ThrowIfNullOrEmpty([NotNull] string arg, [CallerArgumentExpression("arg")] string param = "")
    {
        if (string.IsNullOrEmpty(arg))
            throw new ArgumentNullException(string.IsNullOrEmpty(param) ? nameof(arg) : param);

        return arg;
    }
}
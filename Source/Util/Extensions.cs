namespace PowerliftingSharp.Util;

internal static class Extensions
{
    internal static bool Empty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();
}

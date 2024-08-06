using System.Reactive.Linq;

namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class IterableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        return source.Where(x => x != null).Select(x => x!);
    }

    public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source)
    {
        return source.Where(x => x != null).Select(x => x!);
    }
}
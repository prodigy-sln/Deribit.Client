using System.Reactive.Linq;

namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class SubscriptionObservableExtensions
{
    public static IObservable<T?> ToTypedMessage<T>(this IObservable<SubscriptionMessage> source)
    {
        return source.Select(m => m.Data.ToObject<T>(Constants.JsonSerializer));
    }
}
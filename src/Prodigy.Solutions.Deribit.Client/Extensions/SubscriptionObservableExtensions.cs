using System.Reactive.Linq;
using Prodigy.Solutions.Deribit.Client.Observable;

namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class SubscriptionObservableExtensions
{
    public static IObservable<Exceptional<T?>> ToTypedMessage<T>(this IObservable<SubscriptionMessage> source)
    {
        return source.Select(m => Exceptional.From(() => m.Data.ToObject<T>(Constants.JsonSerializer)));
    }
}
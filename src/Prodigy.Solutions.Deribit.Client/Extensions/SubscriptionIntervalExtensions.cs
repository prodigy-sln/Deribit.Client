using Prodigy.Solutions.Deribit.Client.Subscriptions;

namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class SubscriptionIntervalExtensions
{
    public static string GetApiStringValue(this SubscriptionInterval interval)
    {
        switch (interval)
        {
            case SubscriptionInterval.MilliSeconds100:
                return "100ms";
            case SubscriptionInterval.Aggregate2:
                return "agg2";
            case SubscriptionInterval.Raw:
                return "raw";
            default:
                throw new ArgumentOutOfRangeException(nameof(interval), interval, null);
        }
    }
}
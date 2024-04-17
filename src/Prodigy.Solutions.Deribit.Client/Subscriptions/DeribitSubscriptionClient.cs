using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prodigy.Solutions.Deribit.Client.Authentication;
using Prodigy.Solutions.Deribit.Client.Extensions;
using Prodigy.Solutions.Deribit.Client.Trading;

namespace Prodigy.Solutions.Deribit.Client.Subscriptions;

public class DeribitSubscriptionClient
{
    private readonly DeribitJsonRpcClient _deribitClient;
    private readonly DeribitAuthenticationSession _session;
    private readonly ILogger<DeribitSubscriptionClient> _logger;
    private readonly HashSet<string> _subscribedChannels = new();

    public DeribitSubscriptionClient(DeribitJsonRpcClient deribitClient, DeribitAuthenticationSession session, ILogger<DeribitSubscriptionClient> logger)
    {
        _deribitClient = deribitClient;
        _session = session;
        _logger = logger;
        session.Disconnected += SessionOnDisconnected;
    }

    private void SessionOnDisconnected(object? sender, EventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _logger.LogInformation("Connection lost, reconnecting to channels: {channels}", string.Join(", ", _subscribedChannels));
            await SubscribeAsync(_subscribedChannels.ToArray());
        });
    }

    public async Task<string?> SubscribeAsync(string channel)
    {
        var result = await SubscribeAsync(new[] { channel });
        return result?.SingleOrDefault();
    }

    public Task<string[]?> SubscribeAsync(IReadOnlyList<string> channels)
    {
        return _deribitClient.InvokeAsync<string[]>(GetFullEndpoint("subscribe"), new { channels });
    }

    public async Task<IObservable<OrderResponse>> SubscribeToOrderUpdatesAsync(string instrument, SubscriptionInterval interval = SubscriptionInterval.MilliSeconds100)
    {
        if (interval == SubscriptionInterval.Raw) throw new ArgumentOutOfRangeException(nameof(interval), $"Use {nameof(SubscribeToOrderUpdatesRawAsync)} for raw instead");
        var channel = $"user.orders.{instrument}.{interval.GetApiStringValue()}";
        var observable = await SubscribeToMessagesInternalAsync<OrderResponse[]>(channel);
        return observable.SelectMany(r => r);
    }

    public async Task<IObservable<OrderResponse>> SubscribeToOrderUpdatesRawAsync(string instrument)
    {
        var channel = $"user.orders.{instrument}.raw";
        var observable = await SubscribeToMessagesInternalAsync<OrderResponse>(channel);
        return observable;
    }

    public async Task<IObservable<TradeRespone>> SubscribeToTradesAsync(string instrument, SubscriptionInterval interval = SubscriptionInterval.MilliSeconds100)
    {
        var channel = $"user.trades.{instrument}.{interval.GetApiStringValue()}";
        var observable = await SubscribeToMessagesInternalAsync<TradeRespone[]>(channel);
        return observable.SelectMany(r => r);
    }

    public async Task<IObservable<PlatformStateResponse>> SubscribeToPlatformStateAsync()
    {
        return await SubscribeToMessagesInternalAsync<PlatformStateResponse>("platform_state");
    }

    public async Task<IObservable<UserPortfolioResponse>> SubscribeToUserPortfolioAsync(CurrencyKind currency)
    {
        return await SubscribeToMessagesInternalAsync<UserPortfolioResponse>($"user.portfolio.{currency}".ToLowerInvariant());
    }

    public async Task<IObservable<QuoteResponse>> SubscribeToQuoteAsync(string instrument)
    {
        return await SubscribeToMessagesInternalAsync<QuoteResponse>($"quote.{instrument}");
    }

    private async Task<IObservable<TResult>> SubscribeToMessagesInternalAsync<TResult>(string channel)
    {
        var observable = _deribitClient.GetSubscriptionMessages()?.Where(m => m.Channel == channel).ToTypedMessage<TResult>().WhereNotNull();
        if (observable == null)
        {
            throw new InvalidOperationException("could not subscribe to messages channel");
        }

        if (!_subscribedChannels.Contains(channel))
        {
            var subscribedChannel = await SubscribeAsync(channel);
            if (subscribedChannel != channel)
            {
                throw new Exception($"Subscribed channel '{subscribedChannel}' does not match requested channel '{channel}'.");
            }

            _subscribedChannels.Add(channel);
        }

        return observable;
    }

    private string GetFullEndpoint(string endpoint) => $"{(_session.IsAuthenticated ? "private" : "public")}/{endpoint}";
}

public class QuoteResponse
{
    public decimal BestAskAmount { get; init; }
    public decimal? BestAskPrice { get; init; }
    public decimal BestBidAmount { get; init; }
    public decimal? BestBidPrice { get; init; }
    public required string InstrumentName { get; init; }
    public long Timestamp { get; init; }
    [JsonIgnore]
    public DateTimeOffset DateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
}

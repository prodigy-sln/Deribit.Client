using System.Dynamic;
using System.Globalization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Prodigy.Solutions.Deribit.Client.MarketData;
using StreamJsonRpc;

namespace Prodigy.Solutions.Deribit.Client.Trading;

public class DeribitTradingClient
{
    private readonly DeribitJsonRpcClient _deribitClient;
    private readonly IOptions<DeribitClientOptions> _options;

    public DeribitTradingClient(DeribitJsonRpcClient deribitClient, IOptions<DeribitClientOptions> options)
    {
        _deribitClient = deribitClient;
        _options = options;
    }

    public Task<CreateOrUpdateOrderResult?> PlaceOrderAsync(OrderDirection direction, PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        switch (direction)
        {
            case OrderDirection.Buy:
                return BuyAsync(request, cancellationToken);
            case OrderDirection.Sell:
                return SellAsync(request, cancellationToken);
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public Task<CreateOrUpdateOrderResult?> BuyAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        return _deribitClient.InvokeAsync<CreateOrUpdateOrderResult>("private/buy", request,
            _options.Value.MatchingTokens, cancellationToken);
    }

    public Task<CreateOrUpdateOrderResult?> SellAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        return _deribitClient.InvokeAsync<CreateOrUpdateOrderResult>("private/sell", request,
            _options.Value.MatchingTokens, cancellationToken);
    }

    public Task<CreateOrUpdateOrderResult?> EditAsync(UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        return _deribitClient.InvokeAsync<CreateOrUpdateOrderResult>("private/edit", request,
            _options.Value.MatchingTokens, cancellationToken);
    }

    public Task<OrderResponse?> CancelAsync(string orderId, CancellationToken cancellationToken = default)
    {
        return _deribitClient.InvokeAsync<OrderResponse>("private/cancel", new { order_id = orderId },
            _options.Value.MatchingTokens, cancellationToken);
    }

    public Task<IReadOnlyCollection<OrderResponse>?> CancelAllAsync(CancellationToken cancellationToken = default)
    {
        return _deribitClient.InvokeAsync<IReadOnlyCollection<OrderResponse>>("private/cancel_all",
            new { detailed = true }, _options.Value.MatchingTokens, cancellationToken);
    }

    public Task<int?> CancelAllSimpleAsync(CancellationToken cancellationToken = default)
    {
        return _deribitClient.InvokeAsync<int?>("private/cancel_all", ct: cancellationToken);
    }

    public Task<IReadOnlyCollection<OrderResponse>?> GetOpenOrdersByCurrencyAsync(CurrencyKind currency,
        InstrumentKind? kind, OrderType type = OrderType.All, CancellationToken cancellationToken = default)
    {
        dynamic requestObject = new ExpandoObject();
        requestObject.currency = currency;
        requestObject.type = type;

        if (kind.HasValue) requestObject.kind = kind.Value;
        return _deribitClient.InvokeAsync<IReadOnlyCollection<OrderResponse>>("private/get_open_orders_by_currency",
            requestObject, _options.Value.MatchingTokens, cancellationToken);
    }
}

public class UpdateOrderRequest
{
    public required string OrderId { get; init; }
    public decimal? Amount { get; init; }
    public decimal? Contracts { get; init; }
    public decimal? Price { get; init; }
    public bool? PostOnly { get; init; }
    public bool? ReduceOnly { get; init; }
    public bool? RejectPostOnly { get; init; }
    public OptionsAdvancedOrderType? Advanced { get; init; }
    public decimal? TriggerPrice { get; init; }
    public decimal? TriggerOffset { get; init; }
    public bool? Mmp { get; init; }
    public int? ValidUntil { get; init; }
}

public class PlaceOrderRequest
{
    public required string InstrumentName { get; init; }

    public decimal? Amount { get; init; }

    public decimal? Contracts { get; init; }

    public required OrderType Type { get; init; }

    public string? Label { get; init; }

    public decimal? Price { get; init; }

    public required TimeInForce TimeInForce { get; init; }

    public decimal MaxShow { get; init; }

    public bool PostOnly { get; init; }

    public bool RejectPostOnly { get; init; }

    public bool ReduceOnly { get; init; }

    public decimal? TriggerPrice { get; init; }

    public decimal? TriggerOffset { get; init; }

    public TriggerType? Trigger { get; init; }

    public OptionsAdvancedOrderType? Advanced { get; init; }

    public bool? Mmp { get; init; }

    public long? ValidUntil { get; init; }
}

public enum OptionsAdvancedOrderType
{
    Undefined,
    Usd,
    Implv
}

public enum TriggerType
{
    Undefined,
    IndexPrice,
    MarkPrice,
    LastPrice
}

public enum TimeInForce
{
    Undefined,
    GoodTilCancelled,
    GoodTilDay,
    FillOrKill,
    ImmediateOrCancel
}

public enum OrderType
{
    Undefined,
    All,
    Limit,
    StopLimit,
    TakeLimit,
    Market,
    StopMarket,
    TakeMarket,
    MarketLimit,
    TrailingStop,
    Liquidation
}

public class CreateOrUpdateOrderResult
{
    public required OrderResponse Order { get; init; }

    public TradeRespone[]? Trades { get; init; }
}

public class OrderResponse
{
    public bool? RejectPostOnly { get; init; }
    public string? Label { get; init; }
    public string? QuoteId { get; init; }
    public OrderState OrderState { get; init; }
    public decimal? Usd { get; init; }
    public decimal? Implv { get; init; }
    public decimal? TriggerReferencePrice { get; init; }
    public OrderType OriginalOrderType { get; init; }
    public bool BlockTrade { get; init; }
    public decimal? TriggerPrice { get; init; }
    public bool Api { get; init; }
    public bool Mmp { get; init; }
    public string? TriggerOrderId { get; init; }
    public OrderCancelReason? CancelReason { get; init; }
    public bool Quote { get; init; }
    public bool RiskReducing { get; init; }
    public decimal FilledAmount { get; init; }
    public required string InstrumentName { get; init; }
    public decimal MaxShow { get; init; }
    public string? AppName { get; init; }
    public bool MmpCancelled { get; init; }
    public OrderDirection Direction { get; init; }
    public long LastUpdateTimestamp { get; init; }
    public decimal TriggerOffset { get; init; }
    public string? MmpGroup { get; init; }
    public string? Price { get; init; }

    [JsonIgnore]
    public decimal? PriceDecimal =>
        decimal.TryParse(Price, NumberFormatInfo.InvariantInfo, out var result) ? result : null;

    public bool IsLiquidation { get; init; }
    public bool ReduceOnly { get; init; }
    public decimal Amount { get; init; }
    public bool PostOnly { get; init; }
    public bool Mobile { get; init; }
    public bool Triggered { get; init; }
    public required string OrderId { get; init; }
    public bool Replaced { get; init; }
    public OrderType OrderType { get; init; }
    public TimeInForce TimeInForce { get; init; }
    public bool AutoReplaced { get; init; }
    public string? QuoteSetId { get; init; }
    public decimal? Contracts { get; init; }
    public TriggerType? Trigger { get; init; }
    public bool Web { get; init; }
    public long CreationTimestamp { get; init; }
    public bool IsRebalance { get; init; }
    public decimal AveragePrice { get; init; }
    public OptionsAdvancedOrderType? Advanced { get; init; }
}

public enum OrderDirection
{
    Undefined,
    Buy,
    Sell
}

public enum OrderCancelReason
{
    Undefined,
    UserRequest,
    Autoliquidation,
    CancelOnDisconnect,
    RiskMitigation,
    PmeRiskReduction,
    PmeAccountLocked,
    PositionLocked,
    MmpTrigger,
    MmpConfigCurtailment,
    EditPostOnlyReject
}

public enum OrderState
{
    Undefined,
    Open,
    Filled,
    Rejected,
    Cancelled,
    Untriggered
}

public class TradeRespone
{
    public long Timestamp { get; init; }
    public string? Label { get; init; }
    public decimal Fee { get; init; }
    public TradeType Liquidity { get; init; }
    public decimal IndexPrice { get; init; }
    public bool Api { get; init; }
    public bool Mmp { get; init; }
    public TradeRespone[]? Legs { get; init; }
    public long TradeSeq { get; init; }
    public bool RiskReducing { get; init; }
    public required string InstrumentName { get; init; }
    public string? FeeCurrency { get; init; }
    public OrderDirection Direction { get; init; }
    public required string TradeId { get; init; }
    public long TickDirection { get; init; }
    public decimal ProfitLoss { get; init; }
    public string? MatchingId { get; init; }
    public decimal Price { get; init; }
    public bool ReduceOnly { get; init; }
    public decimal Amount { get; init; }
    public bool PostOnly { get; init; }
    public LiquidationType? Liquidation { get; init; }
    public string? ComboTradeId { get; init; }
    public string? OrderId { get; init; }
    public string? BlockTradeId { get; init; }
    public OrderType OrderType { get; init; }
    public string? ComboId { get; init; }
    public decimal UnderlyingPrice { get; init; }
    public decimal Contracts { get; init; }
    public decimal MarkPrice { get; init; }
    public decimal? Iv { get; init; }
    public OrderState State { get; init; }
    public OptionsAdvancedOrderType? Advanced { get; init; }
}

public enum LiquidationType
{
    Undefined,

    // ReSharper disable InconsistentNaming
    M,
    T,

    MT
    // ReSharper restore InconsistentNaming
}

public enum TradeType
{
    Undefined,

    // ReSharper disable InconsistentNaming
    M,

    T
    // ReSharper restore InconsistentNaming
}
namespace Prodigy.Solutions.Deribit.Client.MarketData;

public class DeribitMarketDataClient
{
    private readonly DeribitJsonRpcClient _deribitClient;

    public DeribitMarketDataClient(DeribitJsonRpcClient deribitClient)
    {
        _deribitClient = deribitClient;
    }
    
    public Task<IReadOnlyCollection<BookSummaryResponse>?> GetBookSummaryByCurrencyAsync(CurrencyKind currency, InstrumentKind? kind = null)
    {
        var request = Utilities.ConvertParametersToExpandoObject(currency, kind);
        return _deribitClient.InvokeAsync<IReadOnlyCollection<BookSummaryResponse>>("public/get_book_summary_by_currency", request);
    }
    
    public Task<BookSummaryResponse?> GetBookSummaryByInstrumentAsync(string instrumentName)
    {
        return _deribitClient.InvokeAsync<BookSummaryResponse>("public/get_book_summary_by_instrument", Utilities.ConvertParametersToExpandoObject(instrumentName));
    }
    
    public async Task<int?> GetContractSizeAsync(string instrumentName)
    {
        var response = await _deribitClient.InvokeAsync<ContractSizeResponse>("public/get_contract_size", Utilities.ConvertParametersToExpandoObject(instrumentName));
        return response?.ContractSize;
    }
    
    public async Task<IReadOnlyCollection<CurrencyResponse>?> GetCurrenciesAsync()
    {
        var response = await _deribitClient.InvokeAsync<IReadOnlyCollection<CurrencyResponse>>("public/get_currencies");
        return response;
    }
    
    public async Task<IReadOnlyCollection<DeliveryPriceResponse>?> GetDeliveryPricesAsync(string indexName, int? offset = null, int? count = null)
    {
        var response = await _deribitClient.InvokeAsync<IReadOnlyCollection<DeliveryPriceResponse>>("public/get_delivery_prices", Utilities.ConvertParametersToExpandoObject(indexName, offset, count));
        return response;
    }
    
    public async Task<FundingChartDataResponse?> GetFundingChartDataAsync(string instrumentName, string length)
    {
        var request = Utilities.ConvertParametersToExpandoObject(instrumentName, length);
        return await _deribitClient.InvokeAsync<FundingChartDataResponse>("public/get_funding_chart_data", request);
    }
    
    public async Task<IReadOnlyCollection<FundingRateHistoryResponse>?> GetFundingRateHistoryAsync(string instrumentName, long startTimestamp, long endTimestamp)
    {
        var requestParameters = Utilities.ConvertParametersToExpandoObject(instrumentName, startTimestamp, endTimestamp);
        var response = await _deribitClient.InvokeAsync<IReadOnlyCollection<FundingRateHistoryResponse>>("public/get_funding_rate_history", requestParameters);
        return response;
    }

    public async Task<decimal?> GetFundingRateValueAsync(string instrumentName, long startTimestamp, long endTimestamp)
    {
        var requestParameters = Utilities.ConvertParametersToExpandoObject(instrumentName, startTimestamp, endTimestamp);
        var response = await _deribitClient.InvokeAsync<decimal>("public/get_funding_rate_value", requestParameters);
        return response;
    }
    
    public async Task<IReadOnlyCollection<HistoricalVolatilityResponse>?> GetHistoricalVolatilityAsync(string currency)
    {
        return await _deribitClient.InvokeAsync<IReadOnlyCollection<HistoricalVolatilityResponse>>("public/get_historical_volatility", Utilities.ConvertParametersToExpandoObject(currency));
    }
    
    public async Task<IndexPriceResponse?> GetIndexPriceAsync(string indexName)
    {
        var request = Utilities.ConvertParametersToExpandoObject(indexName);
        return await _deribitClient.InvokeAsync<IndexPriceResponse>("public/get_index_price", request);
    }
    
    public async Task<IReadOnlyCollection<string>?> GetIndexPriceNamesAsync()
    {
        return await _deribitClient.InvokeAsync<IReadOnlyCollection<string>>("public/get_index_price_names");
    }

    public Task<InstrumentResponse?> GetInstrumentAsync(string instrumentName)
    {
        return _deribitClient.InvokeAsync<InstrumentResponse>("public/get_instrument", Utilities.ConvertParametersToExpandoObject(instrumentName));
    }

    public Task<IReadOnlyCollection<InstrumentResponse>?> GetInstrumentsAsync(CurrencyKind currency, InstrumentKind? kind = null, bool? expired = null)
    {
        var request = Utilities.ConvertParametersToExpandoObject(currency, kind, expired);
        return _deribitClient.InvokeAsync<IReadOnlyCollection<InstrumentResponse>>("public/get_instruments", request);
    }
    
    public async Task<SettlementResponse?> GetLastSettlementsByCurrencyAsync(string currency, string? type = null, int? count = null, string? continuation = null, long? searchStartTimestamp = null)
    {
        var request = Utilities.ConvertParametersToExpandoObject(currency, type, count, continuation, searchStartTimestamp);
        return await _deribitClient.InvokeAsync<SettlementResponse>("public/get_last_settlements_by_currency", request);
    }
    
    public async Task<SettlementResponse?> GetLastSettlementsByInstrumentAsync(
        string instrumentName, 
        SettlementType? type = null, 
        int? count = 20, 
        string? continuation = null, 
        long? searchStartTimestamp = null)
    {
        var request = Utilities.ConvertParametersToExpandoObject(
            instrumentName, 
            type, 
            count, 
            continuation, 
            searchStartTimestamp);
        return await _deribitClient.InvokeAsync<SettlementResponse>("public/get_last_settlements_by_instrument", request);
    }
    
public async Task<LastTradesResponse?> GetLastTradesByCurrencyAsync(string currency, string? kind = null, string? startId = null, string? endId = null, long? startTimestamp = null, long? endTimestamp = null, int? count = 10, string? sorting = null)
{
    var request = Utilities.ConvertParametersToExpandoObject(currency, kind, startId, endId, startTimestamp, endTimestamp, count, sorting);
    return await _deribitClient.InvokeAsync<LastTradesResponse>("public/get_last_trades_by_currency", request);
}

public async Task<LastTradesResponse?> GetLastTradesByCurrencyAndTimeAsync(string currency, long startTimestamp, long endTimestamp, string? kind = null, int? count = 10, string? sorting = null)
{
    var request = Utilities.ConvertParametersToExpandoObject(currency, kind, startTimestamp, endTimestamp, count, sorting);
    return await _deribitClient.InvokeAsync<LastTradesResponse>("public/get_last_trades_by_currency_and_time", request);
}

public async Task<LastTradesResponse?> GetLastTradesByInstrumentAsync(string instrumentName, 
    int? startSeq = null, 
    int? endSeq = null, 
    long? startTimestamp = null, 
    long? endTimestamp = null, 
    int? count = 10, 
    string? sorting = null)
{
    var request = Utilities.ConvertParametersToExpandoObject(instrumentName, 
         startSeq, endSeq, startTimestamp, endTimestamp, count, sorting);
    return await _deribitClient.InvokeAsync<LastTradesResponse>(
        "public/get_last_trades_by_instrument", request);
}

public async Task<LastTradesResponse?> GetLastTradesByInstrumentAndTimeAsync(string instrumentName, long startTimestamp, long endTimestamp, int? count = 10, string? sorting = null)
{
    var request = Utilities.ConvertParametersToExpandoObject(instrumentName, startTimestamp, endTimestamp, count, sorting);
    return await _deribitClient.InvokeAsync<LastTradesResponse>("public/get_last_trades_by_instrument_and_time", request);
}

public async Task<IReadOnlyCollection<IReadOnlyCollection<long>>?> GetMarkPriceHistoryAsync(string instrumentName, long startTimestamp, long endTimestamp)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(instrumentName, startTimestamp, endTimestamp);
    var response = await _deribitClient.InvokeAsync<IReadOnlyCollection<IReadOnlyCollection<long>>>("public/get_mark_price_history", requestParameters);
    return response;
}

public async Task<OrderBookResponse?> GetOrderBookAsync(string instrumentName, int depth)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(instrumentName, depth);
    var response = await _deribitClient.InvokeAsync<OrderBookResponse>("public/get_order_book", requestParameters);
    return response;
}

public async Task<OrderBookResponse?> GetOrderBookByInstrumentIdAsync(int instrumentId, int depth = 1)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(instrumentId, depth);
    return await _deribitClient.InvokeAsync<OrderBookResponse>("public/get_order_book_by_instrument_id", requestParameters);
}

public async Task<IReadOnlyCollection<RfqResponse>?> GetRfqAsync(string currency, string? kind = null)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(currency, kind);
    return await _deribitClient.InvokeAsync<IReadOnlyCollection<RfqResponse>>("public/get_rfqs", requestParameters);
}

public async Task<IReadOnlyCollection<string>?> GetSupportedIndexNamesAsync(string type = "all")
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(type);
    return await _deribitClient.InvokeAsync<IReadOnlyCollection<string>>("public/get_supported_index_names", requestParameters);
}

public async Task<List<TradeVolumeResponse>?> GetTradeVolumesAsync(bool extended = false)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(extended);
    var response = await _deribitClient.InvokeAsync<List<TradeVolumeResponse>>("public/get_trade_volumes", requestParameters);
    return response;
}

public async Task<TradingViewChartDataResponse?> GetTradingViewChartDataAsync(string instrumentName, long startTimestamp, long endTimestamp, string resolution)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(instrumentName, startTimestamp, endTimestamp, resolution);
    return await _deribitClient.InvokeAsync<TradingViewChartDataResponse>("public/get_tradingview_chart_data", requestParameters);
}

public async Task<VolatilityIndexDataResponse?> GetVolatilityIndexDataAsync(string currency, long startTimestamp, long endTimestamp, string resolution)
{
    var requestParameters = Utilities.ConvertParametersToExpandoObject(currency, startTimestamp, endTimestamp, resolution);
    return await _deribitClient.InvokeAsync<VolatilityIndexDataResponse>("public/get_volatility_index_data", requestParameters);
}

public async Task<TickerResponse?> GetTickerAsync(string instrumentName)
{
    return await _deribitClient.InvokeAsync<TickerResponse>("public/ticker", Utilities.ConvertParametersToExpandoObject(instrumentName));
}

public class TickerResponse
{
    public decimal AskIv { get; set; }
    public decimal BestAskAmount { get; set; }
    public decimal BestAskPrice { get; set; }
    public decimal BestBidAmount { get; set; }
    public decimal BestBidPrice { get; set; }
    public decimal BidIv { get; set; }
    public decimal CurrentFunding { get; set; }
    public decimal DeliveryPrice { get; set; }
    public decimal EstimatedDeliveryPrice { get; set; }
    public decimal Funding8h { get; set; }
    public Greeks Greeks { get; set; }
    public decimal IndexPrice { get; set; }
    public string InstrumentName { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestValue { get; set; }
    public decimal LastPrice { get; set; }
    public decimal MarkIv { get; set; }
    public decimal MarkPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal OpenInterest { get; set; }
    public decimal SettlementPrice { get; set; }
    public string State { get; set; }
    public Stats Stats { get; set; }
    public long Timestamp { get; set; }
    public string UnderlyingIndex { get; set; }
    public decimal UnderlyingPrice { get; set; }
}

public class Greeks
{
    public decimal Delta { get; set; }
    public decimal Gamma { get; set; }
    public decimal Rho { get; set; }
    public decimal Theta { get; set; }
    public decimal Vega { get; set; }
}

public class Stats
{
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal PriceChange { get; set; }
    public decimal Volume { get; set; }
    public decimal VolumeUsd { get; set; }
}

public class VolatilityIndexDataResponse
{
    public IReadOnlyCollection<IReadOnlyCollection<decimal>> Data { get; set; }
    public string Continuation { get; set; }
}

public class TradingViewChartDataResponse
{
    public List<decimal> Volume { get; set; }
    public List<decimal> Cost { get; set; }
    public List<long> Ticks { get; set; }
    public string Status { get; set; }
    public List<decimal> Open { get; set; }
    public List<decimal> Low { get; set; }
    public List<decimal> High { get; set; }
    public List<decimal> Close { get; set; }
}

public class TradeVolumeResponse
{
    public decimal CallsVolume { get; set; }
    public decimal CallsVolume7d { get; set; }
    public decimal CallsVolume30d { get; set; }
    public string Currency { get; set; }
    public decimal FuturesVolume { get; set; }
    public decimal FuturesVolume7d { get; set; }
    public decimal FuturesVolume30d { get; set; }
    public decimal PutsVolume { get; set; }
    public decimal PutsVolume7d { get; set; }
    public decimal PutsVolume30d { get; set; }
    public decimal SpotVolume { get; set; }
    public decimal SpotVolume7d { get; set; }
    public decimal SpotVolume30d { get; set; }
}

public class RfqResponse
{
    public decimal TradedVolume { get; set; }
    public decimal Amount { get; set; }
    public string Side { get; set; }
    public long LastRfqTimestamp { get; set; }
    public string InstrumentName { get; set; }
}

public class OrderBookResponse
{
    public class Stat
    {
        public decimal Volume { get; set; }
        public decimal PriceChange { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }
    }

    public long Timestamp { get; set; }
    public Stat Stats { get; set; }
    public string State { get; set; }
    public decimal SettlementPrice { get; set; }
    public decimal OpenInterest { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal MarkPrice { get; set; }
    public decimal LastPrice { get; set; }
    public string InstrumentName { get; set; }
    public decimal IndexPrice { get; set; }
    public decimal Funding8h { get; set; }
    public decimal CurrentFunding { get; set; }
    public IReadOnlyCollection<IReadOnlyCollection<decimal>> Bids { get; set; }
    public decimal BestBidPrice { get; set; }
    public decimal BestBidAmount { get; set; }
    public decimal BestAskPrice { get; set; }
    public decimal BestAskAmount { get; set; }
    public IReadOnlyCollection<IReadOnlyCollection<decimal>> Asks { get; set; }
}
public class LastTradesResponse
{
    public bool HasMore { get; set; }
    public List<Trade> Trades { get; set; }
}

public class Trade
{
    public long TradeSeq { get; set; }
    public string TradeId { get; set; }
    public long Timestamp { get; set; }
    public int TickDirection { get; set; }
    public decimal Price { get; set; }
    public string Direction { get; set; }
    public int Amount { get; set; }
    public string BlockTradeId { get; set; }
    public int BlockTradeLegCount { get; set; }
    public int Contracts { get; set; }
    public decimal IndexPrice { get; set; }
    public string InstrumentName { get; set; }
    public decimal Iv { get; set; }
    public string Liquidation { get; set; }
    public decimal MarkPrice { get; set; }
}
}

public class SettlementResponse
{
    public string? Continuation { get; set; }
    public List<Settlement>? Settlements { get; set; }
}

public class Settlement
{
    public decimal? Funded { get; set; }
    public decimal? Funding { get; set; }
    public decimal? IndexPrice { get; set; }
    public string? InstrumentName { get; set; }
    public decimal? MarkPrice { get; set; }
    public decimal? Position { get; set; } 
    public decimal? ProfitLoss {get; set;}
    public decimal? SessionBankruptcy {get; set;}
    public decimal? SessionProfitLoss {get; set;}
    public decimal? SessionTax {get; set;}
    public decimal? SessionTaxRate {get; set;}
    public decimal? Socialized {get; set;}
    public long Timestamp {get; set;}
    public SettlementType? Type {get; set;}
}

public enum SettlementType
{
    Settlement,
    Delivery,
    Bankruptcy
}

public class IndexPriceResponse
{
    public decimal EstimatedDeliveryPrice { get; set; }
    public decimal IndexPrice { get; set; }
}

public class HistoricalVolatilityResponse
{
    public long Timestamp { get; set; }
    public decimal Volatility { get; set; }
}

public class FundingRateHistoryResponse
{
    public long Timestamp { get; set; }
    public decimal IndexPrice { get; set; }
    public decimal PrevIndexPrice { get; set; }
    public double Interest8H { get; set; }
    public double Interest1H { get; set; }
}

public class FundingChartDataResponse
{
    public decimal CurrentInterest { get; set; }
    public IReadOnlyCollection<FundingChartData> Data { get; set; }
    public decimal Interest8h { get; set; }
}

public class FundingChartData
{
    public decimal IndexPrice { get; set; }
    public decimal Interest8h { get; set; }
    public long Timestamp { get; set; }
}    

public class DeliveryPriceResponse
{
    public required IReadOnlyCollection<DeliveryData> Data { get; set; }
    public int RecordsTotal { get; set; }
}

public class DeliveryData
{
    public required string Date { get; set; }
    public decimal DeliveryPrice { get; set; }
}

public class CurrencyResponse
{
    public required string CoinType { get; init; }
    public required string Currency { get; init; }
    public required string CurrencyLong { get; init; }
    public int FeePrecision { get; set; }
    public bool InCrossCollateralPool { get; set; }
    public int MinConfirmations { get; set; }
    public decimal MinWithdrawalFee { get; set; }
    public decimal WithdrawalFee { get; set; }
    public IReadOnlyCollection<WithdrawalPriority>? WithdrawalPriorities { get; set; }
}

public class WithdrawalPriority
{
    public required string Name { get; init; }
    public decimal Value { get; set; }
}

public class ContractSizeResponse
{
    public int? ContractSize { get; set; }
}

public class BookSummaryResponse
{
    public decimal? AskPrice { get; set; }
    public required string BaseCurrency { get; set; }
    public decimal? BidPrice { get; set; }
    public long CreationTimestamp { get; set; }
    public decimal? CurrentFunding { get; set; }
    public decimal? EstimatedDeliveryPrice { get; set; }
    public decimal? Funding8H { get; set; }
    public decimal? High { get; set; }
    public required string InstrumentName { get; set; }
    public decimal? InterestRate { get; set; }
    public decimal? Last { get; set; }
    public decimal? Low { get; set; }
    public decimal? MarkIv { get; set; }
    public decimal MarkPrice { get; set; }
    public decimal? MidPrice { get; set; }
    public decimal? OpenInterest { get; set; }
    public decimal? PriceChange { get; set; }
    public required string QuoteCurrency { get; set; }
    public required string UnderlyingIndex { get; set; }
    public decimal? UnderlyingPrice { get; set; }
    public decimal Volume { get; set; }
    public decimal VolumeNotional { get; set; }
    public decimal VolumeUsd { get; set; }
}


#nullable enable

namespace Prodigy.Solutions.Deribit.Client.MarketData
{
    partial class DeribitMarketDataClient
    {

        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.BookSummaryResponse>?> GetBookSummaryByCurrencyAsync(Prodigy.Solutions.Deribit.Client.CurrencyKind currency, Prodigy.Solutions.Deribit.Client.MarketData.InstrumentKind? kind)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("kind", kind) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.BookSummaryResponse>?>("public/get_book_summary_by_currency", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.BookSummaryResponse?> GetBookSummaryByInstrumentAsync(string instrumentName)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.BookSummaryResponse?>("public/get_book_summary_by_instrument", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.ContractSizeResponse?> GetContractSizeAsync(string instrumentName)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.ContractSizeResponse?>("public/get_contract_size", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.CurrencyResponse>?> GetCurrenciesAsync()
        {
            
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.CurrencyResponse>?>("public/get_currencies", 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.DeliveryPriceResponse>?> GetDeliveryPricesAsync(string indexName, int? offset, int? count)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("index_name", indexName), ("offset", offset), ("count", count) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.DeliveryPriceResponse>?>("public/get_delivery_prices", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.FundingChartDataResponse?> GetFundingChartDataAsync(string instrumentName, string length)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("length", length) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.FundingChartDataResponse?>("public/get_funding_chart_data", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.FundingRateHistoryResponse>?> GetFundingRateHistoryAsync(string instrumentName, long startTimestamp, long endTimestamp)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.FundingRateHistoryResponse>?>("public/get_funding_rate_history", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<decimal?> GetFundingRateValueAsync(string instrumentName, long startTimestamp, long endTimestamp)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp) });
            return _deribitClient.InvokeAsync<decimal?>("public/get_funding_rate_value", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.HistoricalVolatilityResponse>?> GetHistoricalVolatilityAsync(string currency)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.HistoricalVolatilityResponse>?>("public/get_historical_volatility", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.IndexPriceResponse?> GetIndexPriceAsync(string indexName)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("index_name", indexName) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.IndexPriceResponse?>("public/get_index_price", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<string>?> GetIndexPriceNamesAsync()
        {
            
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<string>?>("public/get_index_price_names", 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.InstrumentResponse?> GetInstrumentAsync(string instrumentName)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.InstrumentResponse?>("public/get_instrument", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.InstrumentResponse>?> GetInstrumentsAsync(Prodigy.Solutions.Deribit.Client.CurrencyKind currency, Prodigy.Solutions.Deribit.Client.MarketData.InstrumentKind? kind, bool? expired)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("kind", kind), ("expired", expired) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.InstrumentResponse>?>("public/get_instruments", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.SettlementResponse?> GetLastSettlementsByCurrencyAsync(string currency, string? type, int? count, string? continuation, long? searchStartTimestamp)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("type", type), ("count", count), ("continuation", continuation), ("search_start_timestamp", searchStartTimestamp) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.SettlementResponse?>("public/get_last_settlements_by_currency", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.SettlementResponse?> GetLastSettlementsByInstrumentAsync(string instrumentName, Prodigy.Solutions.Deribit.Client.MarketData.SettlementType? type, int? count, string? continuation, long? searchStartTimestamp)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("type", type), ("count", count), ("continuation", continuation), ("search_start_timestamp", searchStartTimestamp) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.SettlementResponse?>("public/get_last_settlements_by_instrument", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?> GetLastTradesByCurrencyAsync(string currency, string? kind, string? startId, string? endId, long? startTimestamp, long? endTimestamp, int? count, string? sorting)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("kind", kind), ("start_id", startId), ("end_id", endId), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp), ("count", count), ("sorting", sorting) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?>("public/get_last_trades_by_currency", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?> GetLastTradesByCurrencyAndTimeAsync(string currency, long startTimestamp, long endTimestamp, string? kind, int? count, string? sorting)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp), ("kind", kind), ("count", count), ("sorting", sorting) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?>("public/get_last_trades_by_currency_and_time", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?> GetLastTradesByInstrumentAsync(string instrumentName, int? startSeq, int? endSeq, long? startTimestamp, long? endTimestamp, int? count, string? sorting)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("start_seq", startSeq), ("end_seq", endSeq), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp), ("count", count), ("sorting", sorting) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?>("public/get_last_trades_by_instrument", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?> GetLastTradesByInstrumentAndTimeAsync(string instrumentName, long startTimestamp, long endTimestamp, int? count, string? sorting)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp), ("count", count), ("sorting", sorting) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.LastTradesResponse?>("public/get_last_trades_by_instrument_and_time", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.IReadOnlyCollection<long>>?> GetMarkPriceHistoryAsync(string instrumentName, long startTimestamp, long endTimestamp)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.IReadOnlyCollection<long>>?>("public/get_mark_price_history", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.OrderBookResponse?> GetOrderBookAsync(string instrumentName, int depth)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("depth", depth) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.OrderBookResponse?>("public/get_order_book", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.OrderBookResponse?> GetOrderBookByInstrumentIdAsync(int instrumentId, int depth)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_id", instrumentId), ("depth", depth) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.OrderBookResponse?>("public/get_order_book_by_instrument_id", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.RfqResponse>?> GetRfqAsync(string currency, string? kind)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("kind", kind) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.RfqResponse>?>("public/get_rfqs", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<string>?> GetSupportedIndexNamesAsync(string type)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("type", type) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.IReadOnlyCollection<string>?>("public/get_supported_index_names", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<System.Collections.Generic.List<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.TradeVolumeResponse>?> GetTradeVolumesAsync(bool extended)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("extended", extended) });
            return _deribitClient.InvokeAsync<System.Collections.Generic.List<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.TradeVolumeResponse>?>("public/get_trade_volumes", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.TradingViewChartDataResponse?> GetTradingViewChartDataAsync(string instrumentName, long startTimestamp, long endTimestamp, string resolution)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp), ("resolution", resolution) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.TradingViewChartDataResponse?>("public/get_tradingview_chart_data", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.VolatilityIndexDataResponse?> GetVolatilityIndexDataAsync(string currency, long startTimestamp, long endTimestamp, string resolution)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("currency", currency), ("start_timestamp", startTimestamp), ("end_timestamp", endTimestamp), ("resolution", resolution) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.VolatilityIndexDataResponse?>("public/get_volatility_index_data", request, 500);
        }

                    
        public partial System.Threading.Tasks.Task<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.TickerResponse?> GetTickerAsync(string instrumentName)
        {
            
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] { ("instrument_name", instrumentName) });
            return _deribitClient.InvokeAsync<Prodigy.Solutions.Deribit.Client.MarketData.DeribitMarketDataClient.TickerResponse?>("public/ticker", request, 500);
        }

                    
    }
}

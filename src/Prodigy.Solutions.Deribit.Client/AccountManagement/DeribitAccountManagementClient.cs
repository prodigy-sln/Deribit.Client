using System.Dynamic;
using Prodigy.Solutions.Deribit.Client.Authentication;
using Prodigy.Solutions.Deribit.Client.MarketData;

namespace Prodigy.Solutions.Deribit.Client.AccountManagement;

public class DeribitAccountManagementClient
{
    private readonly DeribitJsonRpcClient _deribitClient;
    private readonly DeribitAuthenticationSession _session;

    public DeribitAccountManagementClient(DeribitJsonRpcClient deribitClient, DeribitAuthenticationSession session)
    {
        _deribitClient = deribitClient;
        _session = session;
    }

    public Task<AccountSummaryResponse> GetAccountSummaryAsync(string currency, int? subAccountId = null,
        bool? extended = null)
    {
        dynamic requestObject = new ExpandoObject();
        requestObject.currency = currency;

        if (subAccountId.HasValue) requestObject.subaccount_id = subAccountId.Value;

        if (extended.HasValue) requestObject.extended = extended.Value;

        return _deribitClient.InvokeAsync<AccountSummaryResponse>("private/get_account_summary", requestObject);
    }

    public Task<IReadOnlyCollection<PositionResult>?> GetPositionsAsync(CurrencyKind currency,
        InstrumentKind? kind = null, int? subaccountId = null)
    {
        dynamic requestObject = new ExpandoObject();
        requestObject.currency = currency;

        if (kind.HasValue) requestObject.kind = kind.Value;
        if (subaccountId.HasValue) requestObject.subaccount_id = subaccountId.Value;

        return _deribitClient.InvokeAsync<IReadOnlyCollection<PositionResult>>("private/get_positions", requestObject);
    }

    public Task<TransactionLogResult> GetTransactionLogAsync(CurrencyKind currency, DateTimeOffset startDate,
        DateTimeOffset endDate, string? query = null, int count = 100, int? continuation = null)
    {
        dynamic requestObject = new ExpandoObject();
        requestObject.currency = currency;
        requestObject.start_timestamp = startDate.ToUnixTimeMilliseconds();
        requestObject.end_timestamp = endDate.ToUnixTimeMilliseconds();
        if (query != null) requestObject.query = query;
        requestObject.count = count;
        if (continuation.HasValue) requestObject.continuation = continuation.Value;
        
        return _deribitClient.InvokeAsync<TransactionLogResult>("private/get_transaction_log", requestObject);
    }

    public async IAsyncEnumerable<TransactionLogEntry> GetAllTransactionLogsAsync(CurrencyKind currency,
        DateTimeOffset startDate, DateTimeOffset endDate, string? query = null, int countPerPage = 100)
    {
        int? continuation = null;
        do
        {
            var result = await GetTransactionLogAsync(currency, startDate, endDate, query, countPerPage, continuation);
            continuation = result.Continuation;
            foreach (var log in result.Logs ?? Array.Empty<TransactionLogEntry>())
                yield return log;
        } while (continuation != null);
    }
}

public class PositionResult
{
    public decimal AveragePrice { get; init; }

    public decimal? AveragePriceUsd { get; init; }

    public decimal Delta { get; init; }

    public PositionDirection Direction { get; init; }

    public decimal? EstimatedLiquidationPrice { get; init; }

    public decimal FloatingProfitLoss { get; init; }

    public decimal FloatingProfitLossUsd { get; init; }

    public decimal? Gamma { get; init; }

    public decimal IndexPrice { get; init; }

    public decimal InitialMargin { get; init; }

    public required string InstrumentName { get; init; }

    public decimal? InterestValue { get; init; }

    public InstrumentKind Kind { get; init; }

    public decimal Leverage { get; init; }

    public decimal MaintenanceMargin { get; init; }

    public decimal MarkPrice { get; init; }

    public decimal OpenOrdersMargin { get; init; }

    public decimal? RealizedFunding { get; init; }

    public decimal RealizedProfitLoss { get; init; }

    public decimal? SettlementPrice { get; init; }

    public decimal Size { get; init; }

    public decimal SizeCurrency { get; init; }

    public decimal? Theta { get; init; }

    public decimal TotalProfitLoss { get; init; }

    public decimal? Vega { get; init; }
}

public enum PositionDirection
{
    Zero,
    Buy,
    Sell
}

public class TransactionLogResult
{
    public int? Continuation { get; set; }
    public IReadOnlyCollection<TransactionLogEntry>? Logs { get; set; }
}

public class TransactionLogEntry
{
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public decimal Cashflow { get; set; }
    public decimal Change { get; set; }
    public decimal? Commission { get; set; }
    public decimal? Contracts { get; set; }
    public required string Currency { get; set; }
    public decimal Equity { get; set; }
    public long Id { get; set; }
    public object? Info { get; set; }
    public string? InstrumentName { get; set; }
    public decimal InterestPl { get; set; }
    public decimal MarkPrice { get; set; }
    public string? OrderId { get; set; }
    public decimal? Position { get; set; }
    public decimal? Price { get; set; }
    public string? PriceCurrency { get; set; }
    public bool ProfitAsCashflow { get; set; }
    public decimal SessionRpl { get; set; }
    public decimal SessionUpl { get; set; }
    public string? Side { get; set; }
    public long Timestamp { get; set; }
    public decimal TotalInterestPl { get; set; }
    public string? TradeId { get; set; }
    public required string Type { get; set; }
    public long UserId { get; set; }
    public string? UserRole { get; set; }
    public long UserSeq { get; set; }
    public required string Username { get; set; }
}

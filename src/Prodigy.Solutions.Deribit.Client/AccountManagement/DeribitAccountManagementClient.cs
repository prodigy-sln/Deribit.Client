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
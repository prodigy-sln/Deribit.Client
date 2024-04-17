namespace Prodigy.Solutions.Deribit.Client.Subscriptions;

public class UserPortfolioResponse
{
    public decimal MaintenanceMargin { get; init; }
    public decimal DeltaTotal { get; init; }
    public decimal OptionsSessionRpl { get; init; }
    public decimal FuturesSessionRpl { get; init; }
    public IReadOnlyDictionary<string, decimal>? DeltaTotalMap { get; init; }
    public decimal SessionUpl { get; init; }
    public decimal FeeBalance { get; init; }
    [Obsolete("EstimatedLiquidationRatio is deprecated, use EstimatedLiquidationRatioMap instead.")]
    public decimal EstimatedLiquidationRatio { get; init; }
    public decimal InitialMargin { get; init; }
    public IReadOnlyDictionary<string, decimal>? OptionsGammaMap { get; init; }
    public decimal FuturesPl { get; init; }
    public required string Currency { get; init; }
    public decimal OptionsValue { get; init; }
    public decimal ProjectedMaintenanceMargin { get; init; }
    public decimal OptionsVega { get; init; }
    public decimal SessionRpl { get; init; }
    public decimal TotalInitialMarginUsd { get; init; }
    public decimal FuturesSessionUpl { get; init; }
    public decimal OptionsSessionUpl { get; init; }
    public bool CrossCollateralEnabled { get; init; }
    public decimal OptionsTheta { get; init; }
    public string? MarginModel { get; init; }
    public decimal OptionsDelta { get; init; }
    public decimal OptionsPl { get; init; }
    public IReadOnlyDictionary<string, decimal>? OptionsVegaMap { get; init; }
    public decimal Balance { get; init; }
    public decimal TotalEquityUsd { get; init; }
    public decimal AdditionalReserve { get; init; }
    public IReadOnlyDictionary<string, decimal>? EstimatedLiquidationRatioMap { get; init; }
    public decimal ProjectedInitialMargin { get; init; }
    public decimal AvailableFunds { get; init; }
    public decimal ProjectedDeltaTotal { get; init; }
    public bool PortfolioMarginingEnabled { get; init; }
    public decimal TotalMaintenanceMarginUsd { get; init; }
    public decimal TotalMarginBalanceUsd { get; init; }
    public decimal TotalPl { get; init; }
    public decimal MarginBalance { get; init; }
    public IReadOnlyDictionary<string, decimal>? OptionsThetaMap { get; init; }
    public decimal AvailableWithdrawalFunds { get; init; }
    public decimal Equity { get; init; }
    public decimal OptionsGamma { get; init; }
}
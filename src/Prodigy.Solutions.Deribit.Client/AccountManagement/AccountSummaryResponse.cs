using Newtonsoft.Json;

namespace Prodigy.Solutions.Deribit.Client.AccountManagement;

public class AccountSummaryResponse
{
    [JsonProperty("delta_total_map")] 
    public IReadOnlyDictionary<string, decimal>? DeltaTotalMap { get; set; }
    [JsonProperty("margin_balance")] 
    public decimal? MarginBalance { get; set; }
    [JsonProperty("futures_session_rpl")] 
    public decimal? FuturesSessionRpl { get; set; }
    [JsonProperty("options_session_rpl")]
    public int? OptionsSessionRpl { get; set; }

    [JsonProperty("estimated_liquidation_ratio_map")]
    public IReadOnlyDictionary<string, decimal>? EstimatedLiquidationRatioMap { get; set; }

    [JsonProperty("session_upl")] 
    public decimal? SessionUpl { get; set; }
    [JsonProperty("email")] 
    public string? Email { get; set; }
    [JsonProperty("system_name")]
    public string? SystemName { get; set; }
    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("interuser_transfers_enabled")]
    public bool? InteruserTransfersEnabled { get; set; }

    [JsonProperty("id")] 
    public int Id { get; set; }

    [JsonProperty("estimated_liquidation_ratio")]
    public decimal? EstimatedLiquidationRatio { get; set; }

    [JsonProperty("options_gamma_map")]
    public IReadOnlyDictionary<string, decimal>? OptionsGammaMap { get; set; }
    [JsonProperty("options_vega")]
    public decimal? OptionsVega { get; set; }
    [JsonProperty("options_value")]
    public decimal? OptionsValue { get; set; }

    [JsonProperty("available_withdrawal_funds")]
    public decimal? AvailableWithdrawalFunds { get; set; }

    [JsonProperty("projected_delta_total")]
    public decimal? ProjectedDeltaTotal { get; set; }

    [JsonProperty("maintenance_margin")]
    public decimal? MaintenanceMargin { get; set; }
    [JsonProperty("total_pl")]
    public decimal? TotalPl { get; set; }
    [JsonProperty("limits")] 
    public LimitsInfo? Limits { get; set; }
    [JsonProperty("options_theta_map")]
    public IReadOnlyDictionary<string, decimal>? OptionsThetaMap { get; set; }

    [JsonProperty("projected_maintenance_margin")]
    public decimal? ProjectedMaintenanceMargin { get; set; }

    [JsonProperty("available_funds")]
    public decimal? AvailableFunds { get; set; }
    [JsonProperty("login_enabled")] 
    public bool? LoginEnabled { get; set; }
    [JsonProperty("options_delta")] 
    public decimal? OptionsDelta { get; set; }
    [JsonProperty("balance")] 
    public decimal? Balance { get; set; }

    [JsonProperty("security_keys_enabled")]
    public bool? SecurityKeysEnabled { get; set; }

    [JsonProperty("referrer_id")] 
    public object ReferrerId { get; set; }
    [JsonProperty("mmp_enabled")]
    public bool? MmpEnabled { get; set; }
    [JsonProperty("equity")] 
    public decimal? Equity { get; set; }
    [JsonProperty("futures_session_upl")] 
    public decimal? FuturesSessionUpl { get; set; }
    [JsonProperty("fee_balance")] 
    public int? FeeBalance { get; set; }
    [JsonProperty("currency")]
    public string? Currency { get; set; }
    [JsonProperty("options_session_upl")]
    public decimal? OptionsSessionUpl { get; set; }

    [JsonProperty("projected_initial_margin")]
    public decimal? ProjectedInitialMargin { get; set; }

    [JsonProperty("options_theta")] 
    public decimal? OptionsTheta { get; set; }
    [JsonProperty("creation_timestamp")]
    public long CreationTimestamp { get; set; }

    [JsonProperty("self_trading_extended_to_subaccounts")]
    public bool? SelfTradingExtendedToSubaccounts { get; set; }

    [JsonProperty("portfolio_margining_enabled")]
    public bool? PortfolioMarginingEnabled { get; set; }

    [JsonProperty("cross_collateral_enabled")]
    public bool? CrossCollateralEnabled { get; set; }

    [JsonProperty("margin_model")] 
    public string? MarginModel { get; set; }

    [JsonProperty("decimal?> options_vega_map")]
    public IReadOnlyDictionary<string, decimal>? OptionsVegaMap { get; set; }

    [JsonProperty("futures_pl")]
    public decimal? FuturesPl { get; set; }
    [JsonProperty("options_pl")] 
    public decimal? OptionsPl { get; set; }
    [JsonProperty("type")] 
    public string? Type { get; set; }

    [JsonProperty("self_trading_reject_mode")]
    public string? SelfTradingRejectMode { get; set; }

    [JsonProperty("initial_margin")]
    public decimal? InitialMargin { get; set; }
    [JsonProperty("spot_reserve")] 
    public int? SpotReserve { get; set; }
    [JsonProperty("delta_total")] 
    public decimal? DeltaTotal { get; set; }
    [JsonProperty("options_gamma")] 
    public decimal? OptionsGamma { get; set; }
    [JsonProperty("session_rpl")] 
    public decimal? SessionRpl { get; set; }

    public class LimitsInfo
    {
        [JsonProperty("non_matching_engine")]
        public NonMatchingEngine? NonMatchingEngine { get; set; }
        [JsonProperty("matching_engine")]
        public MatchingEngine? MatchingEngine { get; set; }
    }

    public class NonMatchingEngine
    {
        [JsonProperty("rate")]
        public int? Rate { get; set; }
        [JsonProperty("burst")]
        public int? Burst { get; set; }
    }

    public class MatchingEngine
    {
        [JsonProperty("rate")]
        public int? Rate { get; set; }
        [JsonProperty("burst")] 
        public int? Burst { get; set; }
    }
}

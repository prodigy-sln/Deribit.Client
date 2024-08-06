using Newtonsoft.Json;

namespace Prodigy.Solutions.Deribit.Client.MarketData;

public class InstrumentResponse
{
    public required string BaseCurrency { get; init; }
    public decimal? BlockTradeCommission { get; init; }
    public decimal? BlockTradeMinTradeAmount { get; init; }
    public decimal? BlockTradeTickSize { get; init; }
    public long ContractSize { get; init; }
    public string? CounterCurrency { get; init; }
    public long CreationTimestamp { get; init; }

    [JsonIgnore] public DateTimeOffset CreationDate => DateTimeOffset.FromUnixTimeMilliseconds(CreationTimestamp);

    public long ExpirationTimestamp { get; init; }

    [JsonIgnore] public DateTimeOffset ExpirationDate => DateTimeOffset.FromUnixTimeMilliseconds(ExpirationTimestamp);

    public long InstrumentId { get; init; }
    public required string InstrumentName { get; init; }
    public InstrumentType InstrumentType { get; init; }
    public bool IsActive { get; init; }
    public InstrumentKind Kind { get; init; }
    public decimal? MakerCommission { get; init; }
    public decimal MaxLeverage { get; init; }
    public decimal? MaxLiquidationCommission { get; init; }
    public decimal? MinTradeAmount { get; init; }
    public string? OptionType { get; init; }
    public string? PriceIndex { get; init; }
    public string? QuoteCurrency { get; init; }
    public bool Rfq { get; init; }
    public string? SettlementCurrency { get; init; }
    public string? SettlementPeriod { get; init; }
    public decimal? Strike { get; init; }
    public decimal? TakerCommission { get; init; }
    public decimal? TickSize { get; init; }
    public IReadOnlyCollection<TickSizeStep>? TickSizeSteps { get; init; }
}
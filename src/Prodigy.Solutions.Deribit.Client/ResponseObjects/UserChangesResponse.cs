using Prodigy.Solutions.Deribit.Client.AccountManagement;
using Prodigy.Solutions.Deribit.Client.MarketData;
using Prodigy.Solutions.Deribit.Client.Trading;

namespace Prodigy.Solutions.Deribit.Client.ResponseObjects;

public class UserChangesResponse
{
    public List<OrderResponse> Orders { get; set; }
    public List<PositionResult> Positions { get; set; }
    public List<DeribitMarketDataClient.Trade> Trades { get; set; }
    public string InstrumentName { get; set; }
}
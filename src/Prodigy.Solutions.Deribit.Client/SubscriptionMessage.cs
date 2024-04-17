using Newtonsoft.Json.Linq;

namespace Prodigy.Solutions.Deribit.Client;

public class SubscriptionMessage
{
    public required string Channel { get; init; }
    
    public required JToken Data { get; init; }
}
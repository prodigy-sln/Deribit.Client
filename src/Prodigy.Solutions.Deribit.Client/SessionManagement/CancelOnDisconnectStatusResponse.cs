using Newtonsoft.Json;

namespace Prodigy.Solutions.Deribit.Client.SessionManagement;

public class CancelOnDisconnectStatusResponse
{
    [JsonProperty("enabled")]
    public bool Enabled { get; init; }

    [JsonProperty("scope")]
    public required string Scope { get; init; }
}

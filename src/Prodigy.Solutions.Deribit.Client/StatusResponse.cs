using System.Text.Json.Serialization;

namespace Prodigy.Solutions.Deribit.Client;

public class StatusResponse
{
    [JsonPropertyName("locked")] public bool Locked { get; init; }

    [JsonPropertyName("locked_currencies")]
    public string[]? LockedCurrencies { get; init; }
}
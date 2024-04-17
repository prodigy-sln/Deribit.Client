using System.Text.Json.Serialization;

namespace Prodigy.Solutions.Deribit.Client.Supporting;

public class HelloResponse
{
    [JsonPropertyName("version")]
    public required Version Version { get; init; }
}

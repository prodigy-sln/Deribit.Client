using Newtonsoft.Json;

namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class AuthResponse
{
    [JsonProperty("access_token")] public required string AccessToken { get; init; }

    [JsonProperty("expires_in")] public int ExpiresIn { get; init; }

    [JsonProperty("refresh_token")] public required string RefreshToken { get; init; }

    [JsonProperty("scope")] public required string Scope { get; init; }

    [JsonProperty("sid")] public string? SessionId { get; init; }

    [JsonProperty("state")] public string? State { get; init; }

    [JsonProperty("token_type")] public required string TokenType { get; init; }
}
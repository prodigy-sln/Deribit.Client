using System.ComponentModel.DataAnnotations;

namespace Prodigy.Solutions.Deribit.Client;

public class DeribitClientOptions
{
    [Required]
    public required Uri Uri { get; init; }

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public TimeSpan WebsocketResponseTimeout { get; init; } = TimeSpan.FromSeconds(30);

    public int NonMatchingTokens = 500;

    public int MatchingTokens = 2500;
}

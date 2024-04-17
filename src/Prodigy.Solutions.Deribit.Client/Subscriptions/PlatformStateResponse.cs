namespace Prodigy.Solutions.Deribit.Client.Subscriptions;

public class PlatformStateResponse
{
    public bool Locked { get; init; }
    public bool Maintenance { get; init; }
    public string? PriceIndex { get; init; }
}
namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class CancellationTokenExtensions
{
    public static CancellationToken Link(this CancellationToken token1, params CancellationToken[] tokens)
    {
        return CancellationTokenSource.CreateLinkedTokenSource([token1, ..tokens]).Token;
    }
}
using StreamJsonRpc;

namespace Prodigy.Solutions.Deribit.Client.Trading;

public class DeribitOrderException(string message, RemoteInvocationException originalException) : Exception(message, originalException)
{
    public int ErrorCode { get; } = originalException.ErrorCode;

    public object? ErrorData { get; } = originalException.ErrorData;
}

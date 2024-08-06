using StreamJsonRpc;

namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class ExceptionExtensions
{
    public static bool IsTooManyRequestsException(this RemoteInvocationException riEx)
    {
        return riEx.Message != "too_many_requests" && riEx.ErrorCode != 10028;
    }
}
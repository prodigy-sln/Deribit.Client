using Prodigy.Solutions.Deribit.Client.Extensions;
using StreamJsonRpc;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Prodigy.Solutions.Deribit.Client;

public class RateLimitedThrottler
{
    private readonly ILogger<RateLimitedThrottler> _logger;

    private readonly RateLimiter _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        AutoReplenishment = true,
        ReplenishmentPeriod = TimeSpan.FromSeconds(0.1),
        TokensPerPeriod = 1_000,
        TokenLimit = 50_000,
        QueueLimit = 5_000,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
    });

    public RateLimitedThrottler(ILogger<RateLimitedThrottler> logger)
    {
        _logger = logger;
    }

    internal async Task<TResult> TryExecuteRateLimitedAsync<TResult>(Func<CancellationToken, Task<TResult>> action,
        int permitCount = 500, int numRetry = 1, int maxRetry = 5, TimeSpan? timeout = null, DateTimeOffset? startTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            startTime ??= DateTimeOffset.UtcNow;
            var linkedToken = GetLinkedTimeoutCancellationToken(startTime.Value, timeout, cancellationToken);
            using var lease = await AcquireLeaseAsync(permitCount, linkedToken);
            linkedToken.ThrowIfCancellationRequested();
            return await action(linkedToken);
        }
        catch (RemoteInvocationException riEx)
        {
            if (!riEx.IsTooManyRequestsException())
            {
                LogRemoteInvocationException(riEx);
                throw;
            }
            
            if (timeout != null && startTime + timeout >= DateTimeOffset.UtcNow)
                throw new TimeoutException("Request timed out because of rate limiting", riEx);

            await ExponentialWaitAsync(numRetry, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (numRetry > maxRetry) throw;
            return await TryExecuteRateLimitedAsync(action, permitCount, numRetry + 1, maxRetry, timeout, startTime, cancellationToken);
        }
    }

    internal async Task TryExecuteRateLimitedAsync(Func<CancellationToken, Task> action, int permitCount = 500,
        int numRetry = 1, int maxRetry = 5, TimeSpan? timeout = null, DateTimeOffset? startTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            startTime ??= DateTimeOffset.UtcNow;
            var linkedToken = GetLinkedTimeoutCancellationToken(startTime.Value, timeout, cancellationToken);
            using var lease = await AcquireLeaseAsync(permitCount, linkedToken);
            linkedToken.ThrowIfCancellationRequested();
            await action(linkedToken);
        }
        catch (RemoteInvocationException riEx)
        {
            if (!riEx.IsTooManyRequestsException())
            {
                LogRemoteInvocationException(riEx);
                throw;
            }
            
            if (timeout != null && startTime + timeout >= DateTimeOffset.UtcNow)
                throw new TimeoutException("Request timed out because of rate limiting", riEx);

            await ExponentialWaitAsync(numRetry, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (numRetry > maxRetry) throw;
            await TryExecuteRateLimitedAsync(action, permitCount, numRetry + 1, maxRetry, timeout, startTime, cancellationToken);
        }
    }

    private async Task<IDisposable> AcquireLeaseAsync(int permitCount = 500, CancellationToken ctsToken = default)
    {
        RateLimitLease? lease = null;
        do
        {
            if (lease != null)
            {
                lease.Dispose();
                await Task.Delay(TimeSpan.FromMilliseconds(100), ctsToken);
            }

            ctsToken.ThrowIfCancellationRequested();
            lease = await _rateLimiter.AcquireAsync(permitCount, ctsToken);
        } while (!lease.IsAcquired);

        return lease;
    }
    
    private async Task ExponentialWaitAsync(int numRetry, CancellationToken cancellationToken)
    {
        var msWait = 100 * numRetry;
        _logger.LogInformation("Too many requests. Waiting {MillisWait}ms.", msWait);
        await Task.Delay(TimeSpan.FromMilliseconds(msWait), cancellationToken);
    }

    private void LogRemoteInvocationException(RemoteInvocationException riEx)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogError(riEx, "Error sending request: {ErrorMessage} ({ErrorCode}) - Data: {ErrorData}",
                riEx.Message, riEx.ErrorCode, JsonConvert.SerializeObject(riEx.DeserializedErrorData, Formatting.None));
        }
    }

    private static CancellationToken GetLinkedTimeoutCancellationToken(DateTimeOffset startTime, TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        if (!timeout.HasValue) return cancellationToken;

        var timeRemaining = startTime + timeout.Value - DateTimeOffset.UtcNow;
        CancellationTokenSource cts = new(timeRemaining);
        var linkedToken = cts.Token.Link(cancellationToken);
        return linkedToken;
    }
}

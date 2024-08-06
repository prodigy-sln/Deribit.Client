using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prodigy.Solutions.Deribit.Client.AccountManagement;
using Prodigy.Solutions.Deribit.Client.Authentication;
using Prodigy.Solutions.Deribit.Client.Extensions;
using Prodigy.Solutions.Deribit.Client.MarketData;
using Prodigy.Solutions.Deribit.Client.SessionManagement;
using Prodigy.Solutions.Deribit.Client.Subscriptions;
using Prodigy.Solutions.Deribit.Client.Supporting;
using Prodigy.Solutions.Deribit.Client.Trading;
using StreamJsonRpc;
using IAsyncDisposable = System.IAsyncDisposable;

namespace Prodigy.Solutions.Deribit.Client;

public class DeribitJsonRpcClient : IAsyncDisposable
{
    public static Version SupportedApiVersion = new(1, 2, 26);
    private readonly DeribitAuthenticationSession _authenticationSession;
    private readonly CancellationTokenSource _cts;
    private readonly List<IDisposable> _disposables = new();
    private readonly ILogger<DeribitJsonRpcClient> _logger;
    private readonly IOptions<DeribitClientOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private TaskCompletionSource _connectedTask = new();
    private JsonRpc? _jsonRpc;

    private Task? _listenerTask;

    private readonly RateLimiter _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        AutoReplenishment = true,
        ReplenishmentPeriod = TimeSpan.FromSeconds(0.1),
        TokensPerPeriod = 1_000,
        TokenLimit = 50_000,
        QueueLimit = 5_000,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
    });

    private DeribitJsonRpcClientServer? _server;
    private ClientWebSocket _socket;

    public DeribitJsonRpcClient(ILogger<DeribitJsonRpcClient> logger, IOptions<DeribitClientOptions> options,
        IServiceProvider serviceProvider, DeribitAuthenticationSession authenticationSession)
    {
        _logger = logger;
        _options = options;
        _serviceProvider = serviceProvider;
        _authenticationSession = authenticationSession;
        _cts = new CancellationTokenSource();
        Authentication = CreateInstance<DeribitAuthenticationClient>();
        Supporting = CreateInstance<DeribitSupportingClient>();
        SessionManagement = CreateInstance<DeribitSessionManagementClient>();
        AccountManagement = CreateInstance<DeribitAccountManagementClient>();
        Trading = CreateInstance<DeribitTradingClient>();
        MarketData = CreateInstance<DeribitMarketDataClient>();
        Subscription = CreateInstance<DeribitSubscriptionClient>();
    }

    public DeribitSupportingClient Supporting { get; }
    public DeribitAuthenticationClient Authentication { get; }
    public DeribitSessionManagementClient SessionManagement { get; }
    public DeribitSubscriptionClient Subscription { get; }
    public DeribitAccountManagementClient AccountManagement { get; }
    public DeribitTradingClient Trading { get; }
    public DeribitMarketDataClient MarketData { get; }

    public bool IsAuthenticated => _authenticationSession.IsAuthenticated;

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
        foreach (var disposable in _disposables) disposable.Dispose();
        _listenerTask?.Dispose();
        _jsonRpc?.Dispose();
    }

    private T CreateInstance<T>()
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, this);
    }

    public IObservable<SubscriptionMessage>? GetSubscriptionMessages()
    {
        return _server?.GetSubscriptionMessages();
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, stoppingToken).Token;
        await ConnectAsync(combinedToken);
        _listenerTask = ListenAsync(combinedToken);
        // await TryGetAccountLimitsAsync();
    }

    // private async Task<long> TryGetAccountLimitsAsync()
    // {
    //     try
    //     {
    //         if (!_authenticationSession.IsAuthenticated)
    //         {
    //             var authResponse = await Authentication.AuthenticateAsync(new AuthRequest()
    //             {
    //                 GrantType = AuthRequestGrantType.ClientSignature
    //             });
    //             
    //             if (string.IsNullOrEmpty(authResponse?.AccessToken))
    //             {
    //                 _logger.LogError("Failed to authenticate");
    //                 return -1;
    //             }
    //         }
    //         
    //         var response = await AccountManagement.GetAccountSummaryAsync("BTC");
    //         return (response.Limits?.NonMatchingEngine?.Rate).GetValueOrDefault();
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error checking account rate limit");
    //     }
    //
    //     return -1;
    // }

    private async Task<ClientWebSocket> ConnectAsync(CancellationToken stoppingToken)
    {
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(_options.Value.Uri, stoppingToken);
        return socket;
    }

    private JsonRpc SetupJsonRpc(ClientWebSocket socket)
    {
        if (socket == null) throw new InvalidOperationException("Socket is null");

        var jsonRpc = new JsonRpc(new WebSocketMessageHandler(socket, new ObjectOnlyJsonRpcFormatter
        {
            JsonSerializer =
            {
                Converters =
                {
                    new StringEnumConverter(new SnakeCaseNamingStrategy())
                },
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            }
        }));

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            var ts = jsonRpc.TraceSource = new TraceSource("trace", SourceLevels.All);
            ts.Listeners.Add(new ConsoleTraceListener());
        }

        _server = CreateInstance<DeribitJsonRpcClientServer>();
        jsonRpc.AddLocalRpcTarget(_server, new JsonRpcTargetOptions
        {
            ClientRequiresNamedArguments = true,
            DisposeOnDisconnect = true
        });

        jsonRpc.StartListening();
        return jsonRpc;
    }

    private async Task? ListenAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Connecting to JSON-RPC websocket");
            using var socket = await ConnectAsync(stoppingToken);
            _socket = socket;
            using var jsonRpc = SetupJsonRpc(socket);
            _jsonRpc = jsonRpc;
            try
            {
                _connectedTask.TrySetResult();
                await jsonRpc.Completion.WithCancellation(stoppingToken);
                _logger.LogInformation("JSON-RPC websocket connection closed");
            }
            catch (OperationCanceledException)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                _connectedTask.TrySetCanceled();
            }
            catch (Exception ex)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error reading",
                        CancellationToken.None);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Error notifying remote party of closure.");
                }

                _connectedTask.TrySetException(ex);
                _logger.LogError(ex, "Error in listener thread.");
            }

            _connectedTask.TrySetResult();
            _connectedTask = new TaskCompletionSource();

            _authenticationSession.SetDisconnected();

            if (stoppingToken.IsCancellationRequested) return;
        }

        _jsonRpc = null;

        // ReSharper disable once FunctionNeverReturns
    }

    public void CloseSocket()
    {
        _connectedTask.TrySetCanceled();
        _ = _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
    }

    public async Task NotifyAsync(string method, object? argument = null, TimeSpan? timeout = null)
    {
        await _connectedTask.Task;
        if (_jsonRpc == null) throw new InvalidOperationException();

        await TryExecuteRateLimitedAsync(
            async _ => await _jsonRpc.NotifyWithParameterObjectAsync(method, argument),
            cancellationToken: _cts.Token);
    }

    public async Task<TResult?> InvokeAsync<TResult>(string method)
    {
        return await InvokeAsync<TResult>(method, new object());
    }

    public async Task<TResult?> InvokeAsync<TResult>(string method, object? argument)
    {
        await _connectedTask.Task;
        if (_jsonRpc == null) throw new InvalidOperationException();

        var result = await TryExecuteRateLimitedAsync(
            InvokeWithParameterObjectAsync,
            cancellationToken: _cts.Token);
        return result;

        async Task<TResult> InvokeWithParameterObjectAsync(CancellationToken ct)
        {
            var response = await _jsonRpc.InvokeWithParameterObjectAsync<TResult>(method, argument, ct);
            return response;
        }
    }

    private async Task<IDisposable> AcquireLeaseAsync(int permitCount = 500, CancellationToken ctsToken = default)
    {
        RateLimitLease? lease = null;
        do
        {
            if (lease != null)
            {
                _logger.LogInformation("lease not acquired. waiting.");
                lease.Dispose();
                await Task.Delay(TimeSpan.FromMilliseconds(100), ctsToken);
            }

            ctsToken.ThrowIfCancellationRequested();
            lease = await _rateLimiter.AcquireAsync(permitCount, ctsToken);
        } while (!lease.IsAcquired);

        return lease;
    }

    private async Task<TResult> TryExecuteRateLimitedAsync<TResult>(Func<CancellationToken, Task<TResult>> action,
        int permitCount = 500, int numRetry = 1, TimeSpan? timeout = null, DateTimeOffset? startTime = null,
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
            if (riEx.IsTooManyRequestsException()) throw;
            if (timeout != null && startTime + timeout >= DateTimeOffset.UtcNow)
                throw new TimeoutException("Request timed out because of rate limiting", riEx);

            await ExponentialWait(numRetry);
            return await TryExecuteRateLimitedAsync(action, permitCount, numRetry + 1, timeout, startTime,
                cancellationToken);
        }
    }

    private async Task TryExecuteRateLimitedAsync(Func<CancellationToken, Task> action, int permitCount = 500,
        int numRetry = 1, TimeSpan? timeout = null, DateTimeOffset? startTime = null,
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
            if (riEx.IsTooManyRequestsException()) throw;
            if (timeout != null && startTime + timeout >= DateTimeOffset.UtcNow)
                throw new TimeoutException("Request timed out because of rate limiting", riEx);

            await ExponentialWait(numRetry);
            await TryExecuteRateLimitedAsync(action, permitCount, numRetry + 1, timeout, startTime, cancellationToken);
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

    private async Task ExponentialWait(int numRetry)
    {
        var msWait = 100 * numRetry;
        _logger.LogInformation("too many requests. waiting {MillisWait}ms.", msWait);
        await Task.Delay(TimeSpan.FromMilliseconds(msWait));
    }
}
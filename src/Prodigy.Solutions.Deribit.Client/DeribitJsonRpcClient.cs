using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prodigy.Solutions.Deribit.Client.AccountManagement;
using Prodigy.Solutions.Deribit.Client.Authentication;
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
    private readonly ILogger<DeribitJsonRpcClient> _logger;
    private readonly IOptions<DeribitClientOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly DeribitAuthenticationSession _authenticationSession;
    private readonly CancellationTokenSource _cts;
    private readonly List<IDisposable> _disposables = new();

    private Task? _listenerTask;
    private JsonRpc? _jsonRpc;
    private TaskCompletionSource _connectedTask = new();
    private DeribitJsonRpcClientServer? _server;

    public static Version SupportedApiVersion = new(1, 2, 26);

    public DeribitSupportingClient Supporting { get; }
    public DeribitAuthenticationClient Authentication { get; }
    public DeribitSessionManagementClient SessionManagement { get; }
    public DeribitSubscriptionClient Subscription { get; }
    public DeribitAccountManagementClient AccountManagement { get; }
    public DeribitTradingClient Trading { get; }
    public DeribitMarketDataClient MarketData { get; }

    public DeribitJsonRpcClient(ILogger<DeribitJsonRpcClient> logger, IOptions<DeribitClientOptions> options, IServiceProvider serviceProvider, DeribitAuthenticationSession authenticationSession)
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

    private T CreateInstance<T>()
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, this);
    }

    public IObservable<SubscriptionMessage>? GetSubscriptionMessages() => _server?.GetSubscriptionMessages();

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, stoppingToken).Token;
        await ConnectAsync(combinedToken);
        _listenerTask = ListenAsync(combinedToken);
    }

    private async Task<ClientWebSocket> ConnectAsync(CancellationToken stoppingToken)
    {
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(_options.Value.Uri, stoppingToken);
        return socket;
    }

    private JsonRpc SetupJsonRpc(ClientWebSocket socket)
    {
        if (socket == null)
        {
            throw new InvalidOperationException("Socket is null");
        }

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
        
        var ts = jsonRpc.TraceSource = new TraceSource("trace", SourceLevels.All);
        ts.Listeners.Add(new ConsoleTraceListener());

        _server = CreateInstance<DeribitJsonRpcClientServer>();
        jsonRpc.AddLocalRpcTarget(_server, new JsonRpcTargetOptions
        {
            ClientRequiresNamedArguments = true,
            DisposeOnDisconnect = true
        });
        
        jsonRpc.StartListening();
        return jsonRpc;
    }

    private ClientWebSocket _socket;

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
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error reading", CancellationToken.None);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Error notifying remote party of closure.");
                }

                _connectedTask.TrySetException(ex);
                _logger.LogError(ex, "Error in listener thread.");
            }

            _connectedTask.TrySetResult();
            _connectedTask = new();
            
            _authenticationSession.SetDisconnected();

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }

        _jsonRpc = null;

        // ReSharper disable once FunctionNeverReturns
    }

    public void CloseSocket()
    {
        _connectedTask.TrySetCanceled();
        _ = _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
    }

    public async Task NotifyAsync(string method, object? argument = null)
    {
        await _connectedTask.Task;
        if (_jsonRpc == null)
        {
            throw new InvalidOperationException();
        }
        
        await _jsonRpc.NotifyWithParameterObjectAsync(method, argument);
    }

    public async Task<TResult?> InvokeAsync<TResult>(string method)
    {
        return await InvokeAsync<TResult>(method, new object() { });
    }

    public async Task<TResult?> InvokeAsync<TResult>(string method, object? argument)
    {
        await _connectedTask.Task;
        if (_jsonRpc == null)
        {
            throw new InvalidOperationException();
        }
        
        return await _jsonRpc.InvokeWithParameterObjectAsync<TResult>(method, argument, _cts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _listenerTask?.Dispose();
        _jsonRpc?.Dispose();
    }
}

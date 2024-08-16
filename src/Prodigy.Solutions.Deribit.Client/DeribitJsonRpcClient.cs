using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Prodigy.Solutions.Deribit.Client.AccountManagement;
using Prodigy.Solutions.Deribit.Client.Authentication;
using Prodigy.Solutions.Deribit.Client.MarketData;
using Prodigy.Solutions.Deribit.Client.SessionManagement;
using Prodigy.Solutions.Deribit.Client.Subscriptions;
using Prodigy.Solutions.Deribit.Client.Supporting;
using Prodigy.Solutions.Deribit.Client.Trading;
using Stateless;
using StreamJsonRpc;
using IAsyncDisposable = System.IAsyncDisposable;

namespace Prodigy.Solutions.Deribit.Client;

public class DeribitJsonRpcClient : IAsyncDisposable
{
    private readonly ILogger<DeribitJsonRpcClient> _logger;
    private readonly IOptions<DeribitClientOptions> _options;
    private readonly DeribitAuthenticationSession _authenticationSession;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _cts;
    private readonly StateMachine<DeribitClientState, string> _stateMachine;
    private readonly AsyncManualResetEvent _requestLock = new(initialState: false);
    private readonly RateLimitedThrottler _throttler;

    private ClientWebSocket? _socket;
    private JsonRpc? _jsonRpc;
    private AuthRequest? _lastAuthRequest;
    private AuthResponse? _lastAuthResponse;
    private CancellationTokenSource _currentSessionToken;
    private DeribitJsonRpcClientServer? _server;

    public DeribitClientState State => _stateMachine.State;

    public DeribitSupportingClient Supporting { get; }
    public DeribitAuthenticationClient Authentication { get; }
    public DeribitSessionManagementClient SessionManagement { get; }
    public DeribitSubscriptionClient Subscription { get; }
    public DeribitAccountManagementClient AccountManagement { get; }
    public DeribitTradingClient Trading { get; }
    public DeribitMarketDataClient MarketData { get; }

    private CancellationToken CurrentToken => _currentSessionToken.Token;
    
    public bool IsAuthenticated => _authenticationSession.IsAuthenticated;

    public DeribitJsonRpcClient(ILogger<DeribitJsonRpcClient> logger, IOptions<DeribitClientOptions> options, DeribitAuthenticationSession authenticationSession, IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, RateLimitedThrottler throttler)
    {
        _logger = logger;
        _options = options;
        _authenticationSession = authenticationSession;
        _serviceProvider = serviceProvider;
        _throttler = throttler;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);
        _currentSessionToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        Authentication = CreateInstance<DeribitAuthenticationClient>();
        Supporting = CreateInstance<DeribitSupportingClient>();
        SessionManagement = CreateInstance<DeribitSessionManagementClient>();
        AccountManagement = CreateInstance<DeribitAccountManagementClient>();
        Trading = CreateInstance<DeribitTradingClient>();
        MarketData = CreateInstance<DeribitMarketDataClient>();
        Subscription = CreateInstance<DeribitSubscriptionClient>();
        
        _stateMachine = new StateMachine<DeribitClientState, string>(DeribitClientState.Disconnected, FiringMode.Immediate)
        {
            RetainSynchronizationContext = true
        };
        ConfigureStateMachine();
        
        _logger.LogInformation("INST DeribitJsonRpcClient");
    }

    public IObservable<SubscriptionMessage>? GetSubscriptionMessages() => _server?.GetSubscriptionMessages();

    public async Task ConnectAsync()
    {
        await _stateMachine.FireAsync(DeribitClientStateTrigger.Connect);
    }

    public async Task AuthenticateAsync(AuthRequest? request = null)
    {
        request ??= new AuthRequest()
        {
            GrantType = AuthRequestGrantType.ClientSignature
        };
        
        await _stateMachine.FireAsync(DeribitClientStateTrigger.Authenticate, request);
    }

    public async Task NotifyAsync(string method, object? argument = null, TimeSpan? timeout = null, int tokens = 500, CancellationToken ct = default)
    {
        if (!_stateMachine.IsInState(DeribitClientState.Connected))
        {
            throw new InvalidOperationException("Not connected.");
        }

        var token = CancellationTokenSource.CreateLinkedTokenSource(CurrentToken, ct).Token;
        
        await _requestLock.WaitAsync(token);

        await _throttler.TryExecuteRateLimitedAsync(
            t => InvokeWithParameterObjectAsync(method, argument, t),
            permitCount: tokens,
            cancellationToken: token);
    }

    public async Task<TResult?> InvokeAsync<TResult>(string method, int tokens = 500, CancellationToken ct = default)
    {
        return await InvokeAsync<TResult>(method, new object(), tokens, ct);
    }

    public async Task<TResult?> InvokeAsync<TResult>(string method, object? argument, int tokens = 500, CancellationToken ct = default)
    {
        if (!_stateMachine.IsInState(DeribitClientState.Connected))
        {
            throw new InvalidOperationException("Not connected.");
        }
        
        var token = CancellationTokenSource.CreateLinkedTokenSource(CurrentToken, ct).Token;
        
        await _requestLock.WaitAsync(token);

        try
        {
            var result = await _throttler.TryExecuteRateLimitedAsync(
                t => InvokeWithParameterObjectAsync<TResult>(method, argument, t),
                permitCount: tokens,
                cancellationToken: token);
            return result;
        }
        catch (ConnectionLostException connectionLostExc)
        {
            await _stateMachine.FireAsync(DeribitClientStateTrigger.Failure, null, connectionLostExc);
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task InvokeWithParameterObjectAsync(string method, object? argument, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (_jsonRpc == null || (_jsonRpc?.IsDisposed).GetValueOrDefault())
        {
            throw new InvalidOperationException();
        }

        ValidateMethodAuthenticated(method);
        
        await _jsonRpc!.InvokeWithParameterObjectAsync(method, argument, ct);
    }

    private void ValidateMethodAuthenticated(string method)
    {
        if (method.StartsWith("private/") && !IsAuthenticated)
        {
            throw new InvalidOperationException("Not authenticated.");
        }
    }

    private async Task<TResult> InvokeWithParameterObjectAsync<TResult>(string method, object? argument, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (_jsonRpc == null || (_jsonRpc?.IsDisposed).GetValueOrDefault())
        {
            throw new InvalidOperationException();
        }

        ValidateMethodAuthenticated(method);
        
        var response = await _jsonRpc!.InvokeWithParameterObjectAsync<TResult>(method, argument, ct);
        return response;
    }

    private void ConfigureStateMachine()
    {
        var authenticateTrigger = _stateMachine.SetTriggerParameters<AuthRequest?>(DeribitClientStateTrigger.Authenticate);
        var authenticatedTrigger = _stateMachine.SetTriggerParameters<AuthResponse>(DeribitClientStateTrigger.Authenticated);
        var failureTrigger = _stateMachine.SetTriggerParameters<StateMachine<DeribitClientState, string>.Transition?, Exception?>(DeribitClientStateTrigger.Failure);

        _stateMachine.Configure(DeribitClientState.Disconnected)
            .Permit(DeribitClientStateTrigger.Connect, DeribitClientState.Connecting)
            .Permit(DeribitClientStateTrigger.Failure, DeribitClientState.Failure);
        
        _stateMachine.Configure(DeribitClientState.Connecting)
            .Permit(DeribitClientStateTrigger.Connected, DeribitClientState.Connected)
            .Permit(DeribitClientStateTrigger.Failure, DeribitClientState.Failure)
            .OnEntryAsync(OnStartConnectingAsync);

        _stateMachine.Configure(DeribitClientState.Connected)
            .Permit(DeribitClientStateTrigger.Failure, DeribitClientState.Failure)
            .Permit(DeribitClientStateTrigger.Authenticate, DeribitClientState.Authenticating)
            .OnEntryAsync(OnConnectedAsync);

        _stateMachine.Configure(DeribitClientState.Authenticating)
            .SubstateOf(DeribitClientState.Connected)
            .Permit(DeribitClientStateTrigger.Failure, DeribitClientState.Failure)
            .Permit(DeribitClientStateTrigger.Authenticated, DeribitClientState.Authenticated)
            .OnEntryFromAsync(authenticateTrigger, OnStartAuthenticatingAsync);

        _stateMachine.Configure(DeribitClientState.Authenticated)
            .SubstateOf(DeribitClientState.Connected)
            .OnEntryFromAsync(authenticatedTrigger, OnAuthenticatedAsync);

        _stateMachine.Configure(DeribitClientState.Failure)
            .Permit(DeribitClientStateTrigger.Connect, DeribitClientState.Connecting)
            .OnEntryFromAsync(failureTrigger, OnFailureAsync);
    }

    private T CreateInstance<T>()
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, this);
    }

    private async Task OnFailureAsync(StateMachine<DeribitClientState, string>.Transition? originalTransition, Exception? exception, StateMachine<DeribitClientState, string>.Transition transition)
    {
        if (originalTransition != null)
        {
            _logger.LogError(exception, "Error while transition from {SourceState} to {TargetState}.",
                originalTransition.Source, originalTransition.Destination);
        }
        else
        {
            _logger.LogError(exception, "An error occurred.");
        }

        if (originalTransition?.Source != DeribitClientState.Disconnected)
        {
            await CleanupConnectionAsync();
            await ConnectAsync();
        }
    }

    private Task OnConnectedAsync()
    {
        _ = StartListenAsync();
        _requestLock.Set();
        return Task.CompletedTask;
    }

    private async Task OnStartConnectingAsync(StateMachine<DeribitClientState, string>.Transition transition)
    {
        try
        {
            _logger.LogInformation("Connecting to Deribit JSON-RPC websocket.");
            if (transition.Source != DeribitClientState.Disconnected)
            {
                await CleanupConnectionAsync();
            }

            await ConnectSocketAsync();
            await _stateMachine.FireAsync(DeribitClientStateTrigger.Connected);
            if (!string.IsNullOrEmpty(_lastAuthResponse?.AccessToken))
            {
                await _stateMachine.FireAsync(DeribitClientStateTrigger.Authenticate, _lastAuthRequest);
            }
        }
        catch (Exception ex)
        {
            await _stateMachine.FireAsync(DeribitClientStateTrigger.Failure, transition, ex);
            throw;
        }
    }

    private async Task StartListenAsync()
    {
        try
        {
            if (_socket is not { State: WebSocketState.Open })
            {
                throw new Exception("Socket not open.");
            }

            _jsonRpc = SetupJsonRpc(_socket);
            _logger.LogInformation("Successfully connected. Listening for JSON-RPC messages.");
            _requestLock.Set();
#pragma warning disable VSTHRD003
            await _jsonRpc!.Completion.WithCancellation(_currentSessionToken.Token);
            _logger.LogInformation("JSON-RPC connection closed. Cancellation requested? {IsCancellationRequested}; Websocket State {WebSocketState}; Close Status {CloseStatus} ({CloseDescription})", _currentSessionToken.IsCancellationRequested, _socket.State, _socket.CloseStatus, _socket.CloseStatusDescription);
            await _currentSessionToken.CancelAsync();
#pragma warning restore VSTHRD003
            _requestLock.Reset();
            _authenticationSession.SetDisconnected();
        }
        catch (OperationCanceledException)
        {
            await _currentSessionToken.CancelAsync();
            await CloseSocketAsync();
        }
        catch (Exception ex)
        {
            await _currentSessionToken.CancelAsync();
            await CloseSocketAsync();
            await _stateMachine.FireAsync(DeribitClientStateTrigger.Failure, null, ex);
        }
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

    private async Task OnStartAuthenticatingAsync(AuthRequest? request, StateMachine<DeribitClientState, string>.Transition transition)
    {
        try
        {
            _logger.LogInformation("Authenticating with Deribit JSON-RPC websocket.");
            _lastAuthRequest = request;
            AuthResponse? response = await Authentication.AuthenticateAsync(request);
            if (!string.IsNullOrEmpty(response?.AccessToken))
            {
                var accountSummary = await this.AccountManagement.GetAccountSummaryAsync("BTC");
                var limits = accountSummary.Limits;
                await _stateMachine.FireAsync(DeribitClientStateTrigger.Authenticated, response);
            }
            else
            {
                await _stateMachine.FireAsync(DeribitClientStateTrigger.AuthenticationFailed);
                throw new AuthenticationException("Authentication failed");
            }
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _stateMachine.FireAsync(DeribitClientStateTrigger.Failure, transition, ex);
            throw;
        }
    }

    private Task OnAuthenticatedAsync(AuthResponse response)
    {
        _lastAuthResponse = response;
        return Task.CompletedTask;
    }

    private async Task CleanupConnectionAsync()
    {
        if (_stateMachine.State == DeribitClientState.Disconnected)
        {
            return;
        }
        
        await _currentSessionToken.CancelAsync();
        _requestLock.Reset();
        await CloseSocketAsync();
        _jsonRpc?.Dispose();
        _socket?.Dispose();
        _socket = null;
        _jsonRpc = null;
    }

    private async Task CloseSocketAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string statusDescription = "Client closing")
    {
        if (_socket?.State == WebSocketState.Open)
        {
            _logger.LogInformation("Closing Deribit JSON-RPC websocket.");
            await _socket.CloseAsync(closeStatus, statusDescription, _cts.Token);
        }
    }

    private async Task ConnectSocketAsync()
    {
        if (!_currentSessionToken.IsCancellationRequested)
        {
            await _currentSessionToken.CancelAsync();
        }

        _currentSessionToken.Dispose();
        _currentSessionToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        _logger.LogInformation("Opening Deribit JSON-RPC websocket.");
        _socket = new ClientWebSocket();
        await _socket.ConnectAsync(_options.Value.Uri, _cts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        await _stateMachine.DeactivateAsync();
    }

    public async Task StopAsync()
    {
        await CleanupConnectionAsync();
    }
}

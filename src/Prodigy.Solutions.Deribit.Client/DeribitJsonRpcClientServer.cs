using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace Prodigy.Solutions.Deribit.Client;

public class DeribitJsonRpcClientServer : IDisposable
{
    private readonly DeribitJsonRpcClient _jsonRpcClient;
    private readonly ILogger<DeribitJsonRpcClientServer> _logger;
    private readonly Subject<SubscriptionMessage> _subscriptionMessagesSubject;
    private readonly IConnectableObservable<SubscriptionMessage> _observable;
    private readonly IDisposable _connection;
    
    private bool _disposed;

    public DeribitJsonRpcClientServer(DeribitJsonRpcClient jsonRpcClient, ILogger<DeribitJsonRpcClientServer> logger)
    {
        _jsonRpcClient = jsonRpcClient;
        _logger = logger;
        _subscriptionMessagesSubject = new();
        _observable = _subscriptionMessagesSubject.Publish();
        _connection = _observable.Connect();
    }

    public IObservable<SubscriptionMessage> GetSubscriptionMessages()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DeribitJsonRpcClientServer));
        return _observable;
    }

    [JsonRpcMethod("heartbeat")]
    public async Task HandleHeartbeatAsync(string? type)
    {
        _logger.LogInformation("heartbeat received");
        if ((type?.Equals("test_request")).GetValueOrDefault())
        {
            await _jsonRpcClient.Supporting.TestAsync();
        }
    }

    [JsonRpcMethod("subscription")]
    public Task HandleSubscriptionAsync(string channel, JToken data)
    {
        _logger.LogInformation("received message for channel {channel}", channel);
        var message = new SubscriptionMessage
        {
            Channel = channel,
            Data = data
        };
        _subscriptionMessagesSubject.OnNext(message);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _disposed = true;
        _connection.Dispose();
        _subscriptionMessagesSubject.Dispose();
    }
}

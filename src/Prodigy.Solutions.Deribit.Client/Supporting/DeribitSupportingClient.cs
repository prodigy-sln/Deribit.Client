using System.Reflection;

namespace Prodigy.Solutions.Deribit.Client.Supporting;

public class DeribitSupportingClient
{
    private readonly DeribitJsonRpcClient _deribitJsonRpcClient;

    public DeribitSupportingClient(DeribitJsonRpcClient deribitJsonRpcClient)
    {
        _deribitJsonRpcClient = deribitJsonRpcClient;
    }

    public async Task<long> GetTimeAsync()
    {
        return await _deribitJsonRpcClient.InvokeAsync<long>("public/get_time");
    }

    public async Task<HelloResponse?> HelloAsync(string? clientName = null, string? version = null)
    {
        var name = clientName ?? Assembly.GetExecutingAssembly().GetName().Name ?? "Prodigy.Solutions.Deribit.Client";
        version ??= (Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0)).ToString();
        return await _deribitJsonRpcClient.InvokeAsync<HelloResponse>("public/hello",
            new { client_name = name, client_version = version });
    }

    public async Task<StatusResponse?> StatusAsync()
    {
        return await _deribitJsonRpcClient.InvokeAsync<StatusResponse>("public/status");
    }

    public async Task<TestResponse?> TestAsync()
    {
        return await _deribitJsonRpcClient.InvokeAsync<TestResponse>("public/test");
    }
}
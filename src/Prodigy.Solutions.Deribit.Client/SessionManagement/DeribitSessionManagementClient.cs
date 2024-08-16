using Prodigy.Solutions.Deribit.Client.Authentication;

namespace Prodigy.Solutions.Deribit.Client.SessionManagement;

public class DeribitSessionManagementClient
{
    private readonly DeribitJsonRpcClient _deribitJsonRpcClient;
    private readonly DeribitAuthenticationSession _session;

    public DeribitSessionManagementClient(DeribitJsonRpcClient deribitJsonRpcClient,
        DeribitAuthenticationSession session)
    {
        _deribitJsonRpcClient = deribitJsonRpcClient;
        _session = session;
    }

    public async Task<bool> SetHeartbeatAsync(int interval)
    {
        return Utilities.ParseStringResponse(
            await _deribitJsonRpcClient.InvokeAsync<string>("public/set_heartbeat", new { interval }));
    }

    public async Task<bool> DisableHeartbeatAsync()
    {
        return Utilities.ParseStringResponse(
            await _deribitJsonRpcClient.InvokeAsync<string>("public/disable_heartbeat"));
    }

    public async Task<bool> EnableCancelOnDisconnectAsync(string scope = "connection")
    {
        Utilities.EnsureAuthenticated(_session);
        return Utilities.ParseStringResponse(
            await _deribitJsonRpcClient.InvokeAsync<string>("private/enable_cancel_on_disconnect", new { scope }));
    }

    public async Task<bool> DisableCancelOnDisconnectAsync(string scope = "connection")
    {
        Utilities.EnsureAuthenticated(_session);
        return Utilities.ParseStringResponse(
            await _deribitJsonRpcClient.InvokeAsync<string>("private/disable_cancel_on_disconnect", new { scope }));
    }

    public Task<CancelOnDisconnectStatusResponse?> GetCancelOnDisconnectAsync(string scope = "connection")
    {
        Utilities.EnsureAuthenticated(_session);
        return _deribitJsonRpcClient.InvokeAsync<CancelOnDisconnectStatusResponse>("private/get_cancel_on_disconnect",
            new { scope });
    }
}
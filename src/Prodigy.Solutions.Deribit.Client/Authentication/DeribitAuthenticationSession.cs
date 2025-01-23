namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class DeribitAuthenticationSession : IAsyncDisposable
{
    private CancellationTokenSource _cts = new();
    private readonly CancellationTokenSource _disposedCancellation = new();
    private Task? _expireTask;

    public bool IsAuthenticated { get; private set; }

    public AuthResponse? LastResponse { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await _disposedCancellation.CancelAsync();
        await CastAndDispose(_cts);
        if (_expireTask != null) await CastAndDispose(_expireTask);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }

    public void SetAuthenticated(AuthResponse authResponse)
    {
        _cts.Cancel();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(_disposedCancellation.Token);
        _expireTask = Task.Delay((authResponse.ExpiresIn - 1) * 1000, _cts.Token)
            .ContinueWith(_ =>
            {
                if (_disposedCancellation.Token.IsCancellationRequested) return;
                Reset();
                OnTokenExpired();
            }, _cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);

        IsAuthenticated = true;
        LastResponse = authResponse;

        OnAuthenticated(authResponse);
    }

    public void SetLoggedOut()
    {
        if (_disposedCancellation.Token.IsCancellationRequested) return;
        Reset();
        OnLoggedOut();
    }

    public void SetDisconnected()
    {
        if (_disposedCancellation.Token.IsCancellationRequested) return;
        Reset();
        OnDisconnected();
    }

    private void Reset()
    {
        _cts.Cancel();
        IsAuthenticated = false;
        LastResponse = null;
    }

    private void OnTokenExpired()
    {
        if (_disposedCancellation.Token.IsCancellationRequested) return;
        TokenExpired?.Invoke(this, EventArgs.Empty);
    }

    private void OnAuthenticated(AuthResponse response)
    {
        if (_disposedCancellation.Token.IsCancellationRequested) return;
        Authenticated?.Invoke(this, response);
    }

    private void OnLoggedOut()
    {
        if (_disposedCancellation.Token.IsCancellationRequested) return;
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisconnected()
    {
        if (_disposedCancellation.Token.IsCancellationRequested) return;
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public event EventHandler TokenExpired;

    public event EventHandler<AuthResponse> Authenticated;

    public event EventHandler LoggedOut;

    public event EventHandler Disconnected;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
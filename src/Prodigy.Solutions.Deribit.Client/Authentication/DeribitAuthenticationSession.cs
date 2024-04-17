namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class DeribitAuthenticationSession : IDisposable
{
    private CancellationTokenSource _cts = new();
    private Task? _expireTask;
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public event EventHandler TokenExpired;

    public event EventHandler<AuthResponse> Authenticated;

    public event EventHandler LoggedOut;

    public event EventHandler Disconnected;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    public bool IsAuthenticated { get; private set; }
    
    public AuthResponse? LastResponse { get; private set; }
    
    public void SetAuthenticated(AuthResponse authResponse)
    {
        _cts.Cancel();
        _cts = new();
        _expireTask = Task.Delay((authResponse.ExpiresIn - 1) * 1000, _cts.Token)
            .ContinueWith(_ =>
            {
                Reset();
                OnTokenExpired();
            }, _cts.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);

        IsAuthenticated = true;
        LastResponse = authResponse;
        
        OnAuthenticated(authResponse);
    }

    public void SetLoggedOut()
    {
        Reset();
        OnLoggedOut();
    }

    public void SetDisconnected()
    {
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
        TokenExpired?.Invoke(this, EventArgs.Empty);
    }

    private void OnAuthenticated(AuthResponse response)
    {
        Authenticated?.Invoke(this, response);
    }

    private void OnLoggedOut()
    {
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _expireTask?.Dispose();
    }
}

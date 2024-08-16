namespace Prodigy.Solutions.Deribit.Client;

public enum DeribitClientState
{
    Disconnected,
    Connecting,
    Connected,
    Authenticating,
    Authenticated,
    Failure
}

public static class DeribitClientStateTrigger
{
    public static string Connect = nameof(Connect);
    internal static string Connected = nameof(Connected);
    public static string Authenticate = nameof(Authenticate);
    internal static string Authenticated = nameof(Authenticated);
    internal static string AuthenticationFailed = nameof(AuthenticationFailed);
    public static string Disconnect = nameof(Disconnect);
    public static string Failure = nameof(Failure);
}

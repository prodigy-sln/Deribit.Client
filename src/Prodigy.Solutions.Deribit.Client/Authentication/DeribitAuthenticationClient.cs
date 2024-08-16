using Microsoft.Extensions.Options;

namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class DeribitAuthenticationClient
{
    private readonly DeribitAuthenticationSession _authSession;
    private readonly DeribitJsonRpcClient _deribitJsonRpcClient;
    private readonly IOptions<DeribitClientOptions> _options;
    private readonly RequestSignatureGenerator _signatureGenerator;

    public DeribitAuthenticationClient(DeribitJsonRpcClient deribitJsonRpcClient,
        RequestSignatureGenerator signatureGenerator, DeribitAuthenticationSession authSession,
        IOptions<DeribitClientOptions> options)
    {
        _deribitJsonRpcClient = deribitJsonRpcClient;
        _signatureGenerator = signatureGenerator;
        _authSession = authSession;
        _options = options;
    }
    
    internal async Task<AuthResponse?> AuthenticateAsync(AuthRequest? request = null)
    {
        request ??= new AuthRequest
        {
            GrantType = AuthRequestGrantType.ClientSignature
        };

        object? requestObject;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        switch (request.GrantType)
        {
            case AuthRequestGrantType.ClientCredentials:
                if (string.IsNullOrWhiteSpace(_options.Value.ClientSecret))
                    throw new ArgumentNullException(nameof(_options.Value.ClientSecret));

                requestObject = new
                {
                    grant_type = "client_credentials", client_id = _options.Value.ClientId,
                    client_secret = _options.Value.ClientSecret, timestamp, state = request.State, scope = request.Scope
                };
                break;
            case AuthRequestGrantType.ClientSignature:
                var signatureInfo = _signatureGenerator.CreateRequestSignature(timestamp, request.SignatureData);
                requestObject = new
                {
                    grant_type = "client_signature", client_id = _options.Value.ClientId,
                    signature = signatureInfo.Signature, nonce = signatureInfo.Nonce, data = signatureInfo.Data,
                    timestamp, state = request.State, scope = request.Scope
                };
                break;
            case AuthRequestGrantType.RefreshToken:
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    throw new ArgumentNullException(nameof(request.RefreshToken));

                requestObject = new
                {
                    grant_type = "refresh_token", client_id = _options.Value.ClientId,
                    refresh_token = request.RefreshToken, timestamp, state = request.State, scope = request.Scope
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.GrantType));
        }

        var response = await _deribitJsonRpcClient.InvokeAsync<AuthResponse>("public/auth", requestObject);
        if (response != null)
        {
            _authSession.SetAuthenticated(response);
        }

        return response;
    }

    internal async Task LogoutAsync()
    {
        Utilities.EnsureAuthenticated(_authSession);
        await _deribitJsonRpcClient.NotifyAsync("private/logout");
        _authSession.SetLoggedOut();
    }
}

namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class AuthRequest
{
    public required AuthRequestGrantType GrantType { get; init; }
    
    public string? RefreshToken { get; init; }
    
    public string? SignatureData { get; init; }
    
    public string? State { get; init; }
    
    public string? Scope { get; init; }
}
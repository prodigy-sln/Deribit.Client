namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class RequestSignatureInfo
{
    public required long TimeStamp { get; init; }
    
    public required string Nonce { get; init; }
    
    public required string Data { get; init; }
    
    public required string Signature { get; init; }
}

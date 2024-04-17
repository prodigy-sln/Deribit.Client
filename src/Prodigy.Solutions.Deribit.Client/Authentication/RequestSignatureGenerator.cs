using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Prodigy.Solutions.Deribit.Client.Authentication;

public class RequestSignatureGenerator : IDisposable
{
    private readonly IOptions<DeribitClientOptions> _options;
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    
    private ulong _nonce;

    public RequestSignatureGenerator(IOptions<DeribitClientOptions> options)
    {
        _options = options;
        InitializeNonce();
    }

    private void InitializeNonce()
    {
        var bytes = new Span<byte>(new byte[sizeof(ulong)]);
        _rng.GetBytes(bytes);
        _nonce = BitConverter.ToUInt64(bytes) / sizeof(ulong);
    }

    private ulong GetNonce()
    {
        unchecked
        {
            _nonce += 1;
        }

        return _nonce;
    }

    public RequestSignatureInfo CreateRequestSignature(long? timestamp = null, string? data = null)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.ClientSecret))
        {
            throw new InvalidOperationException("No secret configured. Cannot create signature.");
        }
        
        data ??= string.Empty;
        timestamp ??= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var nonce = GetNonce().ToString();
        var stringToSign = $"{timestamp}\n{nonce}\n{data}";
        var bytesToSign = Encoding.ASCII.GetBytes(stringToSign);
        var keyBytes = Encoding.ASCII.GetBytes(_options.Value.ClientSecret);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bytesToSign);
        var hexString = string.Concat(Array.ConvertAll(hash, x => x.ToString("x2")));
        return new()
        {
            TimeStamp = timestamp.Value,
            Nonce = nonce,
            Data = data,
            Signature = hexString
        };
    }

    public void Dispose()
    {
        _rng.Dispose();
    }
}

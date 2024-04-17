using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Prodigy.Solutions.Deribit.Client;

public class Constants
{
    public static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault(new ()
    {
        Converters =
        {
            new StringEnumConverter(new SnakeCaseNamingStrategy())
        },
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore
    });
}

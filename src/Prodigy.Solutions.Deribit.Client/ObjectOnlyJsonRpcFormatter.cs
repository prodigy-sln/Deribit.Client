using System.Buffers;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace Prodigy.Solutions.Deribit.Client;

public class ObjectOnlyJsonRpcFormatter : JsonMessageFormatter, IJsonRpcMessageFormatter
{
    void IJsonRpcMessageFormatter.Serialize(IBufferWriter<byte> bufferWriter, JsonRpcMessage message)
    {
        if (message is JsonRpcRequest { ArgumentsList.Count: 0 } request)
        {
            request.ArgumentsList = null;
            request.NamedArguments = new Dictionary<string, object?>();
            request.NamedArgumentDeclaredTypes = new Dictionary<string, Type>();
        }

        base.Serialize(bufferWriter, message);
    }
}
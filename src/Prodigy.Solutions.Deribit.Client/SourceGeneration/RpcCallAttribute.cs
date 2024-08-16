using System;

namespace Prodigy.Solutions.Deribit.Client
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RpcCallAttribute : Attribute
    {
        public RpcCallAttribute(string endpoint, int tokens = 500)
        {
            Endpoint = endpoint;
            Tokens = tokens;
        }

        public string Endpoint { get; }

        public int Tokens { get; } = 500;
    }
}

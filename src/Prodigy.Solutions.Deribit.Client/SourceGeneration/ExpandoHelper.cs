using System;
namespace Prodigy.Solutions.Deribit.Client
{
    internal static class ExpandoHelper {
        internal static System.Dynamic.ExpandoObject CreateExpando(params (string Key, object? Value)[] paramsList)
        {
            var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;

            foreach (var (key, value) in paramsList)
            {
                if (value != null)
                {
                    expando[key] = value;
                }
            }

            return (System.Dynamic.ExpandoObject)expando;
        }
    }
}

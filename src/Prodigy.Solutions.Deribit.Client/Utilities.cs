using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Prodigy.Solutions.Deribit.Client.Authentication;

namespace Prodigy.Solutions.Deribit.Client;

public static class Utilities
{
    public static bool ParseStringResponse(string? response)
    {
        return (response?.Equals("ok", StringComparison.InvariantCultureIgnoreCase)).GetValueOrDefault();
    }

    public static void EnsureAuthenticated(DeribitAuthenticationSession session)
    {
        if (!session.IsAuthenticated) throw new InvalidOperationException("Not authenticated");
    }

    public static ExpandoObject ConvertParametersToExpandoObject(params object?[] args)
    {
        var sf = new StackFrame(1);
        var callerMethod = GetCorrectMethodAlsoForStateMachine(sf.GetMethod());
        var callerParams = callerMethod?.GetParameters();

        if (callerParams == null) throw new InvalidOperationException("could not determine parameters");
        if (callerParams.Length != args.Length)
        {
            var caller = callerMethod?.Name;
            var signature = string.Join(", ", callerParams.Select(p => $"{p.GetType().Name} {p.Name}"));
            throw new ArgumentException($"invalid argument count. required params: {callerParams.Length} [{caller}({signature})]. passed: {args.Length}; Stackframe: {sf}");
        }

        var expando = new ExpandoObject();
        for (var i = 0; i < callerParams.Length; i++)
        {
            var param = callerParams[i];
            if (param.Name == null) throw new ArgumentException("could not determine parameter name");

            var value = args[i];
            if (value != null) ((IDictionary<string, object?>)expando)[param.Name.ToLowerSnakeCase()] = value;
        }

        return expando;
    }
    
    private static MethodBase? GetCorrectMethodAlsoForStateMachine(MethodBase? method)
    {
        if (method == null)
        {
            return null;
        }
        
        if (!(method.DeclaringType?.GetInterfaces().Any(i => i == typeof(IAsyncStateMachine)))
            .GetValueOrDefault())
        {
            return method;
        }
        
        var generatedType = method.DeclaringType;
        var originalType = generatedType?.DeclaringType;
        var foundMethod = originalType?.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Single(m => m.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == generatedType);
        return foundMethod;
    }

    public static string ToLowerSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
            if (char.IsUpper(str[i]))
            {
                if (i != 0 && !char.IsUpper(str[i - 1])) sb.Append("_");
                sb.Append(char.ToLowerInvariant(str[i]));
            }
            else
            {
                sb.Append(str[i]);
            }

        return sb.ToString();
    }
}
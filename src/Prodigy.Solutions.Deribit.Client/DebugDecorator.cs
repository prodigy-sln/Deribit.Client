using System.Reflection;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;

namespace Prodigy.Solutions.Deribit.Client;

public class DebugDecorator<T> : DispatchProxy where T : class
{
    private readonly ILogger<DebugDecorator<T>> _logger;
    
    public T Target { get; private set; }

    public DebugDecorator(ILogger<DebugDecorator<T>> logger)
    {
        _logger = logger;
    }
    
    public static T Decorate(T? target = null)
    {
        // DispatchProxy.Create creates proxy objects
        var proxy = Create<T, DebugDecorator<T>>() as DebugDecorator<T>;

        // If the proxy wraps an underlying object, it must be supplied after creating
        // the proxy.
        proxy!.Target = target ?? Activator.CreateInstance<T>();

        return (proxy as T)!;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        try
        {
            // Perform the logging that this proxy is meant to provide
            _logger.LogInformation("Calling method {TypeName}.{MethodName} with arguments {Arguments}", targetMethod?.DeclaringType?.Name, targetMethod?.Name, args);

            // For this proxy implementation, we still want to call the original API 
            // (after logging has happened), so use reflection to invoke the desired
            // API on our wrapped target object.
            var result = targetMethod?.Invoke(Target, args);

            // A little more logging.
            _logger.LogInformation("Method {TypeName}.{MethodName} returned {ReturnValue}", targetMethod?.DeclaringType?.Name, targetMethod?.Name, result);

            return result;
        }
        catch (TargetInvocationException exc)
        {
            // If the MethodInvoke.Invoke call fails, log a warning and then rethrow the exception
            _logger.LogWarning(exc.InnerException, "Method {TypeName}.{MethodName} threw exception: {Exception}", targetMethod?.DeclaringType?.Name, targetMethod?.Name, exc.InnerException);

            if (exc.InnerException != null)
            {
                throw exc.InnerException;
            }

            throw;
        }
    }
}

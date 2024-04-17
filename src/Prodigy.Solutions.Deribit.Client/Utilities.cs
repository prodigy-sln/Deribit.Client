﻿using System.Diagnostics;
using System.Dynamic;
using Prodigy.Solutions.Deribit.Client.Authentication;

namespace Prodigy.Solutions.Deribit.Client;

public static class Utilities
{
    public static bool ParseStringResponse(string? response) 
        => (response?.Equals("ok", StringComparison.InvariantCultureIgnoreCase)).GetValueOrDefault();

    public static void EnsureAuthenticated(DeribitAuthenticationSession session)
    {
        if (!session.IsAuthenticated)
        {
            throw new InvalidOperationException("Not authenticated");
        }
    }

    public static ExpandoObject ConvertParametersToExpandoObject(params object?[] args) {
        var sf = new StackFrame(1);
        var callerParams = sf.GetMethod()?.GetParameters();
        
        if (callerParams == null)
        {
            throw new InvalidOperationException("could not determine parameters");
        }
        if (callerParams.Length != args.Length) {
            throw new ArgumentException("invalid argument count");
        }
        
        var expando = new ExpandoObject();
        for(var i = 0; i < callerParams.Length; i++) {
            var param = callerParams[i];
            if (param.Name == null)
            {
                throw new ArgumentException("could not determine parameter name");
            }
            
            var value = args[i];
            if (value != null) {
                ((IDictionary<string,object?>)expando)[param.Name] = value;
            }
        }

        return expando;
    }
}

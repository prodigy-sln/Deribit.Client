using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Prodigy.Solutions.Deribit.Client.Generators
{
    [Generator]
    public class RpcClientGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
        
        private class NameSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol>
        {
            public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.ToDisplayString() == y.ToDisplayString();
            }

            public int GetHashCode(INamedTypeSymbol obj)
            {
                return obj.ToDisplayString().GetHashCode();
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var methodsWithSymbol = receiver.CandidateMethods.Select(method =>
                {
                    var model = context.Compilation.GetSemanticModel(method.SyntaxTree);
                    if (model.GetDeclaredSymbol(method) is not IMethodSymbol symbol) return (null, null);
                    return (Model: model, Symbol: symbol);
                })
                .Where(m => m is { Model: not null, Symbol: not null })
                .Select(m => (Model: m.Model!, Symbol: m.Symbol!))
                .GroupBy(m => m.Symbol.ContainingType, comparer: new NameSymbolEqualityComparer());

            foreach (var methodGroup in methodsWithSymbol)
            {
                if (!methodGroup.Any())
                    continue;
                
                Console.WriteLine($"Found {methodGroup.Key.ToDisplayString()}");
                
                var containingType = methodGroup.Key;
                var ns = containingType.ContainingNamespace;
                StringBuilder sb = new StringBuilder();
                sb.Append($@"
#nullable enable

namespace {ns.ToDisplayString()}
{{
    partial class {containingType.Name}
    {{
");
                foreach (var method in methodGroup)
                {
                    var symbol = method.Symbol!;

                    var attribute = symbol.GetAttributes()
                        .FirstOrDefault(ad =>
                        {
                            var displayString = ad.AttributeClass?.ToDisplayString();
                            return displayString is "Prodigy.Solutions.Deribit.Client.RpcCallAttribute" or "RpcCall";
                        });

                    if (attribute == null) continue;
                    
                    var returnTypeSymbol = symbol.ReturnType as INamedTypeSymbol;
                    if (returnTypeSymbol == null) continue;
                    var returnType = returnTypeSymbol.ToDisplayString();
                    var returnTypeInvoke = returnTypeSymbol.TypeArguments[0].ToDisplayString();
                    var endpoint = attribute.ConstructorArguments.FirstOrDefault(a => a.Type?.Name == "String").Value?.ToString();
                    var tokens = attribute.ConstructorArguments.FirstOrDefault(a => a.Type?.Name == "Int32").Value?.ToString();
                    var methodParameters = string.Join(", ", symbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                    var isPrivate = (endpoint?.StartsWith("private/")).GetValueOrDefault();
                    var privateCheckSource = isPrivate ? "Utilities.EnsureAuthenticated(_authenticationSession);" : "";

                    if (symbol.Parameters.Length == 0)
                    {
                        var methodSource = $@"
        public partial {returnType} {symbol.Name}({methodParameters})
        {{
            {privateCheckSource}
            return _deribitClient.InvokeAsync<{returnTypeInvoke}>(""{endpoint}"", {tokens});
        }}

                    ";
                        sb.Append(methodSource);
                    }
                    else
                    {
                        var parameters = string.Join(", ", symbol.Parameters.Select(p => $"(\"{ToLowerSnakeCase(p.Name)}\", {p.Name})"));;
                        var methodSource = $@"
        public partial {returnType} {symbol.Name}({methodParameters})
        {{
            {privateCheckSource}
            var request = Prodigy.Solutions.Deribit.Client.ExpandoHelper.CreateExpando(new (string Key, object? Value)[] {{ {parameters} }});
            return _deribitClient.InvokeAsync<{returnTypeInvoke}>(""{endpoint}"", request, {tokens});
        }}

                    ";
                        sb.Append(methodSource);
                    }
                }
                sb.Append($@"
    }}
}}
");

                context.AddSource($"{containingType.Name}_RpcCall.g", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }
    
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (!(syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax) ||
                    methodDeclarationSyntax.AttributeLists.Count <= 0) return;

                if (methodDeclarationSyntax.AttributeLists.Any(l =>
                        l.Attributes.Any(a => a.Name is IdentifierNameSyntax { Identifier.Text: "RpcCall" })))
                {
                    CandidateMethods.Add(methodDeclarationSyntax);
                }
            }
        }

        public static string ToLowerSnakeCase(string str)
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
}

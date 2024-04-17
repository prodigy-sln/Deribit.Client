using System.Reflection;
using System.Runtime.Serialization;

namespace Prodigy.Solutions.Deribit.Client.Extensions;

public static class EnumExtensions<T> where T : struct, Enum
{
    private static readonly Dictionary<Type, Dictionary<T, string>> EnumNames = new();
    
    private static readonly Type EnumType = typeof(T);
    
    public static string SerializeAsString(T value)
    {
        if (!EnumNames.TryGetValue(EnumType, out var names))
        {
            names = EnumNames[EnumType] = new();
        }

        if (names.TryGetValue(value, out var serialized))
        {
            return serialized;
        }

        var stringValue = value.ToString();
        var enumMemberValue = EnumType.GetTypeInfo().DeclaredMembers
            .SingleOrDefault(x => x.Name == stringValue)
            ?.GetCustomAttribute<EnumMemberAttribute>(false)?.Value;
        return names[value] = enumMemberValue ?? stringValue;
    }
}

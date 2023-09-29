using System;
using System.Reflection;
using System.Text;
using MassTransit;

namespace Bus.RedeliveryCountError.Sample.Formatters;

public sealed class DefaultEntityNameFormatter : IEntityNameFormatter
{
    public static char NamespaceSeparator => ':';
    
    public string FormatEntityName<T>() => GetFormattedFullName(typeof(T));

    private static string GetFormattedFullName(Type type)
    {
        var typeInfo = type.GetTypeInfo();
        if (typeInfo.GenericTypeArguments.Length > 0)
            return GetFormattedFullName(typeInfo.GenericTypeArguments[0]);

        var sb = new StringBuilder();
        if (typeInfo.Namespace != null)
        {
            var ns = typeInfo.Namespace;
            sb.Append(ns);
            sb.Append(NamespaceSeparator);
        }

        sb.Append(typeInfo.Name);

        return sb.ToString();
    }
}
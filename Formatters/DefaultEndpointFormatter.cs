using System;
using Bus.RedeliveryCountError.Sample.Abstractions;
using MassTransit;

namespace Bus.RedeliveryCountError.Sample.Formatters;

public sealed class DefaultEndpointFormatter : IEndpointFormatter
{
    private readonly IEntityNameFormatter _entityNameFormatter;

    public DefaultEndpointFormatter(IEntityNameFormatter entityNameFormatter)
    {
        _entityNameFormatter = entityNameFormatter;
    }
    
    public char NamespaceSeparator => ':';

    public Uri FormatEndpointUri<T>() => GetFormattedSendUri(_entityNameFormatter.FormatEntityName<T>());
    
    private static Uri GetFormattedSendUri(string entityName) => new($"queue:{entityName}");
}
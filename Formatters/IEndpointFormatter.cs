using System;

namespace Bus.RedeliveryCountError.Sample.Formatters;

public interface IEndpointFormatter
{
    char NamespaceSeparator { get; }
    Uri FormatEndpointUri<T>();
}
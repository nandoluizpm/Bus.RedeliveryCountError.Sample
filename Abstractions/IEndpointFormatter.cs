using System;

namespace Bus.RedeliveryCountError.Sample.Abstractions;

public interface IEndpointFormatter
{
    char NamespaceSeparator { get; }
    Uri FormatEndpointUri<T>();
}
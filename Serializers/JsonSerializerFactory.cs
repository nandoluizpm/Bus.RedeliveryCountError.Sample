using System;
using System.Net.Mime;
using MassTransit;

namespace Bus.RedeliveryCountError.Sample.Serializers;

public class JsonSerializerFactory : ISerializerFactory
{
    private readonly Lazy<SystemTextJsonCustomSerializer> _serializer;

    public JsonSerializerFactory()
    {
        _serializer = new Lazy<SystemTextJsonCustomSerializer>(() => new SystemTextJsonCustomSerializer());
    }

    public ContentType ContentType => SystemTextJsonCustomSerializer.JsonContentType;

    public IMessageSerializer CreateSerializer()
    {
        return _serializer.Value;
    }

    public IMessageDeserializer CreateDeserializer()
    {
        return _serializer.Value;
    }
}
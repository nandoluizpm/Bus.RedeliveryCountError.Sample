using System;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text.Json;
using Bus.RedeliveryCountError.Sample.Extensions;
using MassTransit;
using MassTransit.Serialization;

namespace Bus.RedeliveryCountError.Sample.Serializers;

public class SystemTextJsonCustomSerializer :
    IMessageDeserializer,
    IMessageSerializer,
    IObjectDeserializer
{
    public static readonly ContentType JsonContentType = SystemTextJsonMessageSerializer.JsonContentType;
    public static readonly JsonSerializerOptions Options = SystemTextJsonMessageSerializer.Options;
    private readonly SystemTextJsonMessageSerializer _originalSerializer = SystemTextJsonMessageSerializer.Instance;

    public SystemTextJsonCustomSerializer(ContentType contentType = null)
    {
        ContentType = contentType ?? JsonContentType;
    }

    public ContentType ContentType { get; }

    public void Probe(ProbeContext context) => _originalSerializer.Probe(context);
    
    public ConsumeContext Deserialize(ReceiveContext receiveContext)
    {
        return new BodyConsumeContext(receiveContext, Deserialize(receiveContext.Body, receiveContext.TransportHeaders, receiveContext.InputAddress));
    }
    
    /// <summary>
    /// Overrides deserialize method with a tiny modification that fixes error in case of BOM existence.
    /// </summary>
    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri destinationAddress = null)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(body.GetStream(), Options); //Uses Stream to avoid deserialization errors due to BOM when it comes in the payload. 
            if (envelope == null)
                throw new SerializationException("Message envelope not found");

            var messageContext = new EnvelopeMessageContext(envelope, this);

            var messageTypes = envelope.MessageType ?? Array.Empty<string>();

            var serializerContext = new SystemTextJsonSerializerContext(this, Options, ContentType, messageContext, messageTypes, envelope);

            return serializerContext;
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SerializationException("An error occured while deserializing the message envelope", ex);
        }
    }

    public MessageBody GetMessageBody(string text) => _originalSerializer.GetMessageBody(text);

    public MessageBody GetMessageBody<T>(SendContext<T> context)
        where T : class
        => _originalSerializer.GetMessageBody(context);

    public T DeserializeObject<T>(object value, T defaultValue = default)
        where T : class
        => _originalSerializer.DeserializeObject(value, defaultValue);

    public T? DeserializeObject<T>(object value, T? defaultValue = null)
        where T : struct
        => _originalSerializer.DeserializeObject(value, defaultValue);

    public MessageBody SerializeObject(object value)
        => _originalSerializer.SerializeObject(value);
}
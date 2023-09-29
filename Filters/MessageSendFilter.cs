using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bus.RedeliveryCountError.Sample.Filters;

public class MessageSendFilter<T> :  IFilter<SendContext<T>> 
    where T : class
{
    private readonly ILogger<MessageSendFilter<T>> _logger;

    public MessageSendFilter(ILogger<MessageSendFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        _logger.LogWarning("-> Send filter executing.");
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
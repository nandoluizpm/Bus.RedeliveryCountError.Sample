using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bus.RedeliveryCountError.Sample.Filters;

public class MessageConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<MessageSendFilter<T>> _logger;

    public MessageConsumeFilter(ILogger<MessageSendFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogWarning("-> Consume filter executing.");
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
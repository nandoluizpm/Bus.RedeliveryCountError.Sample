using System.Threading.Tasks;
using Bus.RedeliveryCountError.Sample.Formatters;
using Bus.RedeliveryCountError.Sample.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bus.RedeliveryCountError.Sample.Consumers;

public class StartCommandConsumer : IConsumer<StartCommand>
{
    private readonly ILogger<StartCommandConsumer> _logger;
    private readonly IBusControl _bus;
    private readonly IEntityNameFormatter _entityNameFormatter;
    private readonly IEndpointFormatter _endpointFormatter;

    public StartCommandConsumer(ILogger<StartCommandConsumer> logger, IBusControl bus, IEntityNameFormatter entityNameFormatter, IEndpointFormatter endpointFormatter)
    {
        _logger = logger;
        _bus = bus;
        _entityNameFormatter = entityNameFormatter;
        _endpointFormatter = endpointFormatter;
    }

    public async Task Consume(ConsumeContext<StartCommand> context)
    {
        _logger.LogDebug("Initiating RoutingSlip...");
        
        var builder = new RoutingSlipBuilder(NewId.NextGuid());
        builder.AddActivity(_entityNameFormatter.FormatEntityName<CommandA>(), _endpointFormatter.FormatEndpointUri<CommandA>(), 
            new CommandA
            {
                MillisecondsDelay = context.Message.ActivityAExecutionDelay,
                ShouldThrowException = context.Message.ActivityAThrowOnExecution,
                ShouldThrowInCompensation = context.Message.ActivityAThrowOnCompensation,
                ShouldThrowIgnoredException = context.Message.ShouldThrowIgnoredException
            });
        builder.AddActivity(_entityNameFormatter.FormatEntityName<CommandB>(), _endpointFormatter.FormatEndpointUri<CommandB>(), 
            new CommandB
            {
                MillisecondsDelay = context.Message.ActivityBExecutionDelay,
                ShouldThrowException = context.Message.ActivityBThrowOnExecution,
                ShouldThrowInCompensation = context.Message.ActivityBThrowOnCompensation,
                ShouldThrowIgnoredException = context.Message.ShouldThrowIgnoredException
            });
        builder.AddActivity(_entityNameFormatter.FormatEntityName<CommandC>(), _endpointFormatter.FormatEndpointUri<CommandC>(), 
            new CommandC
            {
                MillisecondsDelay = context.Message.ActivityCExecutionDelay,
                ShouldThrowException = context.Message.ActivityCThrowOnExecution,
                ShouldThrowIgnoredException = context.Message.ShouldThrowIgnoredException
            });

        var routingSlip = builder.Build();
        
        var endpoint = await _bus.GetSendEndpoint(routingSlip.GetNextExecuteAddress()!);
        await endpoint.Send(routingSlip, routingSlip.GetType(), Pipe.New<SendContext>(_ => { }));
    }
}
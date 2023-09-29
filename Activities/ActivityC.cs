using System;
using System.Threading.Tasks;
using Bus.RedeliveryCountError.Sample.Exceptions;
using Bus.RedeliveryCountError.Sample.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bus.RedeliveryCountError.Sample.Activities;

public class ActivityC : IExecuteActivity<CommandC>
{
    private readonly ILogger<ActivityC> _logger;

    public ActivityC(ILogger<ActivityC> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> Execute(ExecuteContext<CommandC> context)
    {
        _logger.LogInformation("Executing transaction {StepName} | Retry: {RetryCount} - Redelivery Count: {RedeliveryCount}", nameof(ActivityC), context.GetRetryCount(), context.GetRedeliveryCount());
            
        if (context.Arguments.ShouldThrowException)
        {
            if(context.Arguments.ShouldThrowIgnoredException) throw new BusinessException($"An error has occurred executing {nameof(CommandC)}");

            throw new Exception($"An error has occurred executing {nameof(CommandC)}");
        }
            
        await Task.Delay(context.Arguments.MillisecondsDelay);

        return context.Completed();
    }
}
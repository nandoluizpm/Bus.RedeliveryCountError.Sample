using System;
using System.Threading.Tasks;
using Bus.RedeliveryCountError.Sample.Exceptions;
using Bus.RedeliveryCountError.Sample.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bus.RedeliveryCountError.Sample.Activities;

public class ActivityA : IActivity<CommandA, CompensateCommandA>
{
    private readonly ILogger<ActivityA> _logger;

    public ActivityA(ILogger<ActivityA> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> Execute(ExecuteContext<CommandA> context)
    {
        _logger.LogInformation("Executing transaction {StepName} | Retry: {RetryCount} - Redelivery Count: {RedeliveryCount}", nameof(ActivityA), context.GetRetryCount(), context.GetRedeliveryCount());
            
        if (context.Arguments.ShouldThrowException)
        {
            if(context.Arguments.ShouldThrowIgnoredException) throw new BusinessException($"An error has occurred executing {nameof(CommandA)}");

            throw new Exception($"An error has occurred executing {nameof(CommandA)}");
        }

        await Task.Delay(context.Arguments.MillisecondsDelay);

        return context.Completed(new CompensateCommandA
        {
            ShouldThrow = context.Arguments.ShouldThrowInCompensation,
            ShouldThrowIgnoredException = context.Arguments.ShouldThrowIgnoredException
        });
    }

    public Task<CompensationResult> Compensate(CompensateContext<CompensateCommandA> context)
    {
        _logger.LogCritical("Compensating transaction {StepName} | Retry: {RetryCount} - Redelivery Count: {RedeliveryCount}", nameof(ActivityA), context.GetRetryCount(), context.GetRedeliveryCount());
            
        if (context.Log.ShouldThrow)
        {
            if (context.Log.ShouldThrowIgnoredException) throw new BusinessException($"An error has occurred executing {nameof(CompensateCommandA)}");
                
            throw new Exception($"An error has occurred executing {nameof(CompensateCommandA)}");
        }

        return Task.FromResult(context.Compensated());
    }
}
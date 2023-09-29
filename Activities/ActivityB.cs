using System;
using System.Threading.Tasks;
using Bus.RedeliveryCountError.Sample.Exceptions;
using Bus.RedeliveryCountError.Sample.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bus.RedeliveryCountError.Sample.Activities;

public class ActivityB : IActivity<CommandB, CompensateCommandB>
{
    private readonly ILogger<ActivityB> _logger;

    public ActivityB(ILogger<ActivityB> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> Execute(ExecuteContext<CommandB> context)
    {
        _logger.LogInformation("Executing transaction {StepName} | Retry: {RetryCount} - Redelivery Count: {RedeliveryCount}", nameof(ActivityB), context.GetRetryCount(), context.GetRedeliveryCount());
            
        if (context.Arguments.ShouldThrowException)
        {
            if(context.Arguments.ShouldThrowIgnoredException) throw new BusinessException($"An error has occurred executing {nameof(CommandB)}");

            throw new Exception($"An error has occurred executing {nameof(CommandB)}");
        }

        await Task.Delay(context.Arguments.MillisecondsDelay);

        return context.Completed(new CompensateCommandB
        {
            ShouldThrow = context.Arguments.ShouldThrowInCompensation,
            ShouldThrowIgnoredException = context.Arguments.ShouldThrowIgnoredException
        });
    }

    public Task<CompensationResult> Compensate(CompensateContext<CompensateCommandB> context)
    {
        _logger.LogCritical("Compensating transaction {StepName} | Retry: {RetryCount} - Redelivery Count: {RedeliveryCount}", nameof(ActivityB), context.GetRetryCount(), context.GetRedeliveryCount());
            
        if (context.Log.ShouldThrow)
        {
            if (context.Log.ShouldThrowIgnoredException) throw new BusinessException($"An error has occurred executing {nameof(CompensateCommandB)}");
                
            throw new Exception($"An error has occurred executing {nameof(CompensateCommandB)}");
        }

        return Task.FromResult(context.Compensated());
    }
}
using Hangfire;
using Hangfire.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bus.RedeliveryCountError.Sample.Hangfire;

public class HangfireServiceScopeJobActivator : AspNetCoreJobActivator
{
    readonly IServiceScopeFactory _serviceScopeFactory;

    public HangfireServiceScopeJobActivator(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override JobActivatorScope BeginScope(JobActivatorContext context) => new HangfireServiceJobActivatorScope(context, _serviceScopeFactory.CreateScope());
}
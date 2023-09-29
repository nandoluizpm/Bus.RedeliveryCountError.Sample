using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Bus.RedeliveryCountError.Sample.Hangfire;

public class HangfireServiceJobActivatorScope : JobActivatorScope, IServiceProvider
{
    readonly JobActivatorContext _context;
    readonly IServiceScope _serviceScope;

    public HangfireServiceJobActivatorScope(JobActivatorContext context, IServiceScope serviceScope)
    {
        _context = context;
        _serviceScope = serviceScope;
    }

    public override object Resolve(Type type) => ActivatorUtilities.GetServiceOrCreateInstance(this, type);

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(JobActivatorContext)) return _context;
        return _serviceScope.ServiceProvider.GetService(serviceType);
    }
}
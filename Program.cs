using System;
using MassTransit;
using Bus.RedeliveryCountError.Sample.Activities;
using Bus.RedeliveryCountError.Sample.Consumers;
using Bus.RedeliveryCountError.Sample.Exceptions;
using Bus.RedeliveryCountError.Sample.Filters;
using Bus.RedeliveryCountError.Sample.Formatters;
using Bus.RedeliveryCountError.Sample.Hangfire;
using Bus.RedeliveryCountError.Sample.Messages;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(x => x.AddConsole());
builder.Services.AddSingleton<JobActivator, HangfireServiceScopeJobActivator>();
builder.Services.AddTransient<IEntityNameFormatter, DefaultEntityNameFormatter>();
builder.Services.AddTransient<IEndpointFormatter, DefaultEndpointFormatter>();
builder.Services.AddHangfire((_, gconfig) => gconfig
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer(opt =>
{
    opt.ServerName = $"sample-server-{Guid.NewGuid()}";
    opt.Queues = new[] {"sample-queue", "default"};
    opt.WorkerCount = 1;
    opt.ServerTimeout = TimeSpan.FromSeconds(30);
    opt.HeartbeatInterval = TimeSpan.FromSeconds(10);
    opt.ServerCheckInterval = TimeSpan.FromSeconds(30);
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer(typeof(StartCommandConsumer));
    x.AddActivity(typeof(ActivityA));
    x.AddActivity(typeof(ActivityB));
    x.AddExecuteActivity(typeof(ActivityC));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var entityNameFormatter = context.GetRequiredService<IEntityNameFormatter>();
        
        cfg.Host(new Uri("amqp://guest:guest@localhost:5672"));
        cfg.PublishTopology.BrokerTopologyOptions = PublishBrokerTopologyOptions.MaintainHierarchy;
        cfg.SendTopology.ConfigureErrorSettings = settings => settings.AutoDelete = true;
        cfg.SendTopology.ConfigureDeadLetterSettings = settings => settings.AutoDelete = true;
        cfg.MessageTopology.SetEntityNameFormatter(entityNameFormatter);
        
        cfg.UseHangfireScheduler("Hangfire", opt =>
        {
            opt.WorkerCount = 1;
            opt.ServerTimeout = TimeSpan.FromSeconds(30);
            opt.HeartbeatInterval = TimeSpan.FromSeconds(10);
            opt.ServerCheckInterval = TimeSpan.FromSeconds(30);
        });
        cfg.UseServiceScope(context);
        cfg.UseSendFilter(typeof(MessageSendFilter<>), context);
        cfg.UseConsumeFilter(typeof(MessageConsumeFilter<>), context);
        
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<StartCommand>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            endpoint.ConfigureConsumer(context, typeof(StartCommandConsumer));
        });
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CommandA>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CompensateCommandA>(), compensateEndpoint =>
            {
                ConfigureEndpoint(compensateEndpoint);
                endpoint.ConfigureActivity(compensateEndpoint, context, typeof(ActivityA));
            });
        });
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CommandB>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CompensateCommandB>(), compensateEndpoint =>
            {
                ConfigureEndpoint(compensateEndpoint);
                endpoint.ConfigureActivity(compensateEndpoint, context, typeof(ActivityB));
            });
        });
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CommandC>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            endpoint.ConfigureExecuteActivity(context, typeof(ActivityC));
        });
    });
});

var app = builder.Build();

var storage = app.Services.GetRequiredService<JobStorage>();
var routes = app.Services.GetRequiredService<RouteCollection>();
app.Map("/hangfire", x =>
    x.UseMiddleware<HangfireDashboardMiddleware>(storage, new DashboardOptions
    {
        DisplayStorageConnectionString = false,
        IsReadOnlyFunc = _ => false,
        IgnoreAntiforgeryToken = true,
        Authorization = Array.Empty<IDashboardAuthorizationFilter>()
    }, routes));

app.MapPost("/start", async (StartCommand command, IBusControl bus, IEndpointFormatter formatter) =>
{
    var sendEndpoint = await bus.GetSendEndpoint(formatter.FormatEndpointUri<StartCommand>());
    await sendEndpoint.Send(command, command.GetType(), Pipe.New<SendContext>(_ => { }));
});

app.Run();

static void ConfigureEndpoint(IRabbitMqReceiveEndpointConfigurator endpoint)
{
    endpoint.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15)));
    endpoint.UseMessageRetry(r =>
    {
        r.Immediate(5);
        r.Ignore(typeof(BusinessException));
    });
    endpoint.UseInMemoryOutbox();
    endpoint.DiscardFaultedMessages();
    endpoint.DiscardSkippedMessages();
    endpoint.ConfigureConsumeTopology = false;
}
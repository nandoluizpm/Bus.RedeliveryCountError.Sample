using System;
using MassTransit;
using Bus.RedeliveryCountError.Sample.Abstractions;
using Bus.RedeliveryCountError.Sample.Activities;
using Bus.RedeliveryCountError.Sample.Consumers;
using Bus.RedeliveryCountError.Sample.Exceptions;
using Bus.RedeliveryCountError.Sample.Filters;
using Bus.RedeliveryCountError.Sample.Formatters;
using Bus.RedeliveryCountError.Sample.Messages;
using Bus.RedeliveryCountError.Sample.Serializers;
using MassTransit.Courier.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(x => x.AddConsole());
builder.Services.AddTransient<IEntityNameFormatter, DefaultEntityNameFormatter>();
builder.Services.AddTransient<IEndpointFormatter, DefaultEndpointFormatter>();
builder.Services.AddScoped<StartCommandConsumer>();
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
        var defaultFactory = new JsonSerializerFactory();
        cfg.AddSerializer(defaultFactory);
        cfg.AddDeserializer(defaultFactory, true);
        cfg.UseHangfireScheduler("Hangfire", opt =>
        {
            opt.WorkerCount = 1;
            opt.ServerTimeout = TimeSpan.FromSeconds(30);
            opt.HeartbeatInterval = TimeSpan.FromSeconds(10);
            opt.ServerCheckInterval = TimeSpan.FromSeconds(30);
        });
        cfg.UseServiceScope(context);
        cfg.MessageTopology.SetEntityNameFormatter(entityNameFormatter);
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CommandA>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CompensateCommandA>(), compensateEndpoint =>
            {
                ConfigureEndpoint(compensateEndpoint);
                endpoint.ConfigureConsumeTopology = false;
                endpoint.ConfigureActivity(compensateEndpoint, context, typeof(ActivityA));
            });
        });
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CommandB>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CompensateCommandB>(), compensateEndpoint =>
            {
                ConfigureEndpoint(compensateEndpoint);
                endpoint.ConfigureConsumeTopology = false;
                endpoint.ConfigureActivity(compensateEndpoint, context, typeof(ActivityB));
            });
        });
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<CommandC>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            endpoint.ConfigureConsumeTopology = false;
            endpoint.ConfigureExecuteActivity(context, typeof(ActivityC));
        });
        ConfigureRoutingSlipEventCorrelation();
        cfg.UseSendFilter(typeof(MessageSendFilter<>), context);
        cfg.UseConsumeFilter(typeof(MessageConsumeFilter<>), context);
        cfg.ReceiveEndpoint(entityNameFormatter.FormatEntityName<StartCommand>(), endpoint =>
        {
            ConfigureEndpoint(endpoint);
            endpoint.ConfigureConsumer(context, typeof(StartCommandConsumer));
            endpoint.ConfigureConsumeTopology = false;
        });
    });
});

var app = builder.Build();

app.MapPost("/start", async (StartCommand command, IBusControl bus, IEndpointFormatter formatter) =>
{
    var sendEndpoint = await bus.GetSendEndpoint(formatter.FormatEndpointUri<StartCommand>());
    await sendEndpoint.Send(command, command.GetType(), Pipe.New<SendContext>(_ => { }));
});

app.Run();

static void ConfigureEndpoint(IReceiveEndpointConfigurator endpoint)
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
}

static void ConfigureRoutingSlipEventCorrelation()
{
    var topology = GlobalTopology.Send;
    topology.UseCorrelationId<RoutingSlipCompleted>(x => x.TrackingNumber);
    topology.UseCorrelationId<RoutingSlipFaulted>(x => x.TrackingNumber);
    topology.UseCorrelationId<RoutingSlipActivityCompleted>(x => x.ExecutionId);
    topology.UseCorrelationId<RoutingSlipActivityFaulted>(x => x.ExecutionId);
    topology.UseCorrelationId<RoutingSlipActivityCompensated>(x => x.ExecutionId);
    topology.UseCorrelationId<RoutingSlipActivityCompensationFailed>(x => x.ExecutionId);
    topology.UseCorrelationId<RoutingSlipCompensationFailed>(x => x.TrackingNumber);
    topology.UseCorrelationId<RoutingSlipTerminated>(x => x.TrackingNumber);
    topology.UseCorrelationId<RoutingSlipRevised>(x => x.TrackingNumber);
}
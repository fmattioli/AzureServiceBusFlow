using AzureServiceBusFlow.Extensions;
using AzureServiceBusFlow.Sample.Queues.Commands;
using AzureServiceBusFlow.Sample.Queues.Events;
using Mattioli.Configurations.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });


builder.Services.AddAzureServiceBus(cfg => cfg
    .UseConnectionString("Endpoint=sb://mattioli.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ymrYkVJB0BLKYzrNO3/f9+voH/UJKfSlK+ASbBE/0RU=")
    .AddProducer<ExampleCommand1>(p => p
        .EnsureQueueExists("command-queue-one")
        .WithCommandProducer()
        .ToQueue("command-queue-one"))
    .AddProducer<ExampleCommand2>(p => p
        .EnsureQueueExists("command-queue-two")
        .WithEventProducer()
        .ToQueue("command-queue-two"))
    .AddProducer<ExampleEvent1>(p => p
        .EnsureQueueExists("event-queue-one")
        .WithEventProducer()
        .ToQueue("event-queue-one"))
    .AddProducer<ExampleEvent2>(p => p
        .EnsureQueueExists("event-queue-two")
        .WithEventProducer()
        .ToQueue("event-queue-two"))
    .AddConsumer(c => c
        .FromQueue("command-queue-one")
        .AddHandler<ExampleCommand1, CommandExemple1Handler>()
        .AddHandler<ExampleCommand1, CommandExampleTwoHandlersPerOneMessageHandler>())
    .AddConsumer(c => c
        .FromQueue("command-queue-two")
        .AddHandler<ExampleCommand2, CommandExample2Handler>())
    .AddConsumer(c => c
        .FromQueue("event-queue-one")
        .AddHandler<ExampleEvent1, EventExample1Handler>()
        .AddHandler<ExampleEvent1, EventExampleTwoHandlersPerOneMessageHandler>())
    .AddConsumer(c => c
        .FromQueue("event-queue-two")
        .AddHandler<ExampleEvent2, EventExample2Handler>()
        .AddHandler<ExampleEvent2, EventExample2Handler>())
    );

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

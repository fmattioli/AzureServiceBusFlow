using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Configurations.ApiTestTwo.Command;
using AzureServiceBusFlow.Extensions;
using Mattioli.Configurations.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });


builder.Services.AddAzureServiceBus(cfg => cfg
    .UseConnectionString("")
    .AddProducer<IServiceBusMessage>(p => p
        .EnsureTopicExists("topic-one")
        .EnsureSubscriptionExists("topic-one", "api-subscription-two")
        .AddCommandProducer()
        .ToTopic("topic-one"))
    .AddConsumer(c => c
        .FromTopic("topic-one", "api-subscription-two")
        .AddHandler<PedidoCriadoCommand, PedidoCriadoHandler>()
        .AddHandler<PedidoCriadoCommand, PedidoRecebidoCommandHandler>())
    );

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

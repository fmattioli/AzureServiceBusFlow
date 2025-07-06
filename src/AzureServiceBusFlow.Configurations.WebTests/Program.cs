using AzureServiceBusFlow.Configurations.Abstractions;
using AzureServiceBusFlow.Configurations.Extensions;
using AzureServiceBusFlow.Configurations.Producers.Abstractions;
using AzureServiceBusFlow.Configurations.Producers.Implementations;
using AzureServiceBusFlow.Configurations.WebTests.Command;
using Mattioli.Configurations.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });


builder.Services.AddAzureServiceBus(cfg => cfg
    .UseConnectionString("")
    .AddProducer<IServiceBusMessage>(p => p
        .WithCommandProducer()
        .ToQueue("queue-one")
        .ToTopic("meu-topico"))
    .AddConsumer<PedidoCriadoCommand>(c => c
        .FromQueue("queue-one")
        .FromTopic("meu-topico", "minha-subscription")
        .AddHandler<PedidoCriadoHandler>()
        .AddHandler<OutroHandlerQualquerCommandHandler>())
    );

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

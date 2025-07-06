using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Configurations.WebTests.Command;
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
        .WithCommandProducer()
        .ToQueue("queue-one")
        .ToTopic("meu-topico"))
    .AddConsumer(c => c
        .FromQueue("queue-one")
        .FromTopic("meu-topico", "minha-subscription")
        .AddHandler<PedidoCriadoCommand, PedidoCriadoHandler>()
        .AddHandler<PedidoRecebidoCommand, PedidoRecebidoCommandHandler>())
    );

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

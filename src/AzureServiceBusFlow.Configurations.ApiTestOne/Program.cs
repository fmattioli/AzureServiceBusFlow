using AzureServiceBusFlow.Configurations.ApiTestOne.Command;
using AzureServiceBusFlow.Extensions;
using Mattioli.Configurations.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });


builder.Services.AddAzureServiceBus(cfg => cfg
    .UseConnectionString("")
    .AddProducer(p => p
        .EnsureQueueExists("queue-one")
        .WithCommandProducer()
        .WithEventProducer()
        .ToQueue("queue-one"))
    .AddConsumer(c => c
        .FromQueue("queue-one")
        .AddHandler<ExampleCommand1, PedidoCriadoHandler>()
        .AddHandler<ExampleCommand1, PedidoRecebidoCommandHandler>())
    );

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

using AzureServiceBusFlow.Configurations.Abstractions;
using AzureServiceBusFlow.Configurations.Extensions;
using AzureServiceBusFlow.Configurations.WebTests.Command;
using AzureServiceBusFlow.Configurations.WebTests.Config;
using Coderaw.Settings.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

builder.Services.AddSingleton<ICommandProducer, CommandProducer>();

builder.Services.AddAzureServiceBus(cfg => cfg
    .UseConnectionString("")
    .AddProducer<IServiceBusMessage>(p => p
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

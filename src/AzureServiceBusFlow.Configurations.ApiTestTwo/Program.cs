using AzureServiceBusFlow.Configurations.ApiTestTwo.Command;
using AzureServiceBusFlow.Extensions;
using Mattioli.Configurations.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });


builder.Services.AddAzureServiceBus(cfg => cfg
    .UseConnectionString("Endpoint=sb://mattioli.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ymrYkVJB0BLKYzrNO3/f9+voH/UJKfSlK+ASbBE/0RU=")
    .AddProducer(p => p
        .EnsureTopicExists("topic-one")
        .EnsureSubscriptionExists("topic-one", "api-subscription-one")
        .WithCommandProducer()
        .ToTopic("topic-one"))
    .AddConsumer(c => c
        .FromTopic("topic-one", "api-subscription-one")
        .AddHandler<ExampleCommand1, PedidoCriadoHandler>()
        .AddHandler<ExampleCommand1, PedidoRecebidoCommandHandler>())
    );

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

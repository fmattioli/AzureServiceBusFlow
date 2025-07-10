using AzureServiceBusFlow.Configurations.ApiTestTwo.Command;
using AzureServiceBusFlow.Extensions;
using Mattioli.Configurations.Transformers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });



var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

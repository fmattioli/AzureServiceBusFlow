using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Sample.Commands;
using AzureServiceBusFlow.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;

namespace AzureServiceBusFlow.Sample.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class CommandController(ICommandProducer<ExampleCommand1> _producerOne, ICommandProducer<ExampleCommand2> _producerTwo) : ControllerBase
    {
        [EndpointDescription("Este endpoint produz um comando que será manipulado por dois handlers, isso é para casos onde você precisa de mais de um handler para um único comando.")]
        [HttpPost("command-example-one")]
        public async Task<IActionResult> Example1(CancellationToken cancellationToken)
        {
            ExampleCommand1 command = new()
            {
                ExampleMessage = new ExampleMessage
                {
                    Cliente = "jose",
                    Id = Guid.NewGuid(),
                    Valor = 1111
                }
            };

            await _producerOne.ProduceCommandAsync(command, cancellationToken);
            return Ok();
        }

        [EndpointDescription("Este endpoint produz um comando que será manipulado por um unico handler, isso é para casos onde você precisa de um handler apenas para um comando.")]
        [HttpPost("command-example-two")]
        public async Task<IActionResult> Example2(CancellationToken cancellationToken)
        {
            ExampleCommand2 command = new()
            {
                ExampleMessage = new ExampleMessage
                {
                    Cliente = "jose",
                    Id = Guid.NewGuid(),
                    Valor = 1111
                }
            };

            await _producerTwo.ProduceCommandAsync(command, cancellationToken);
            return Ok();
        }
    }
}

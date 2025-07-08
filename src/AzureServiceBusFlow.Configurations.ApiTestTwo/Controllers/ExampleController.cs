using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Configurations.ApiTestTwo.Command;
using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusFlow.Configurations.ApiTestTwo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController(ICommandProducer _producer) : ControllerBase
    {
        [HttpPost("example1")]
        public async Task<IActionResult> Example1(CancellationToken cancellationToken)
        {
            ExampleCommand1 command = new()
            {
                Category = new ExampleMessage
                {
                    Cliente = "jose",
                    Id = Guid.NewGuid(),
                    Valor = 1111
                },
            };

            await _producer.ProduceCommandAsync(command, cancellationToken);
            return Ok();
        }

        [HttpPost("example2")]
        public async Task<IActionResult> Example2(CancellationToken cancellationToken)
        {
            ExampleCommand2 command = new()
            {
                Name = "Name",
                MessageCreatedDate = DateTime.UtcNow,
                RoutingKey = Guid.NewGuid().ToString()
            };

            await _producer.ProduceCommandAsync(command, cancellationToken);
            return Ok();
        }

        [HttpPost("example3")]
        public async Task<IActionResult> Example3(CancellationToken cancellationToken)
        {
            ExampleCommand3 command = new()
            {
                Name = "Name",
                MessageCreatedDate = DateTime.UtcNow,
                RoutingKey = Guid.NewGuid().ToString()
            };

            await _producer.ProduceCommandAsync(command, cancellationToken);
            return Ok();
        }
    }
}

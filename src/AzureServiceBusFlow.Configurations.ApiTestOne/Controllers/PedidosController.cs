using AzureServiceBusFlow.Configurations.ApiTestOne.Command;
using AzureServiceBusFlow.Producers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusFlow.Configurations.ApiTestOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController(ICommandProducer _producer) : ControllerBase
    {
        [HttpPost("example1")]
        public async Task<IActionResult> CriarPedido(CancellationToken cancellationToken)
        {
            PedidoCriadoCommand command = new()
            {
                Category = new PedidoCriado
                {
                    Cliente = "jose",
                    Id = Guid.NewGuid(),
                    Valor = 1111
                },
                MessageCreatedDate = DateTime.UtcNow,
                RoutingKey = Guid.NewGuid().ToString()
            };

            await _producer.ProduceCommandAsync(command, cancellationToken);
            return Ok();
        }

        [HttpPost("example2")]
        public async Task<IActionResult> CriarPedido2(CancellationToken cancellationToken)
        {
            PedidoRecebidoCommand command = new()
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

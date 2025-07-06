using AzureServiceBusFlow.Configurations.Producers.Abstractions;
using AzureServiceBusFlow.Configurations.WebTests.Command;

using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusFlow.Configurations.WebTests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController(ICommandProducer _producer) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CriarPedido(PedidoCriado request)
        {
            PedidoCriadoCommand command = new()
            {
                Category = request,
                CommandCreatedDate = DateTime.UtcNow,
                RoutingKey = request.Id.ToString()
            };

            await _producer.ProduceCommandAsync(command);
            return Ok();
        }
    }
}

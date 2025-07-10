using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Sample.Queues.Events;
using AzureServiceBusFlow.Sample.Queues.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusFlow.Sample.Queues.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController(IEventProducer<ExampleEvent1> _producerOne, IEventProducer<ExampleEvent2> _producerTwo) : ControllerBase
    {
        [EndpointDescription("Este endpoint produz um evento que será manipulado por dois handlers, isso é para casos onde você precisa de mais de um handler para um único evento.")]
        [HttpPost("event-example-one")]
        public async Task<IActionResult> Example1(CancellationToken cancellationToken)
        {
            ExampleEvent1 @event = new()
            {
                ExampleMessage = new ExampleMessage
                {
                    Cliente = "jose",
                    Id = Guid.NewGuid(),
                    Valor = 1111
                }
            };

            await _producerOne.ProduceEventAsync(@event, cancellationToken);
            return Ok();
        }

        [EndpointDescription("Este endpoint produz um evento que será manipulado por um unico handler, isso é para casos onde você precisa de um handler apenas para um evento.")]
        [HttpPost("event-example-two")]
        public async Task<IActionResult> Example2(CancellationToken cancellationToken)
        {
            ExampleEvent2 command = new()
            {
                ExampleMessage = new ExampleMessage
                {
                    Cliente = "jose",
                    Id = Guid.NewGuid(),
                    Valor = 1111
                }
            };

            await _producerTwo.ProduceEventAsync(command, cancellationToken);

            return Ok();
        }
    }
}

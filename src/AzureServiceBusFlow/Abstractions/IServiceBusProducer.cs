using AzureServiceBusFlow.Models;

namespace AzureServiceBusFlow.Abstractions
{
    /// <summary>
    /// Defines a producer responsible for sending messages to Azure Service Bus.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be sent.</typeparam>
    public interface IServiceBusProducer<in TMessage> where TMessage : class, IServiceBusMessage
    {
        /// <summary>
        /// Sends a message to the Azure Service Bus without any additional options.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task ProduceAsync(TMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a message to the Azure Service Bus with custom producer options,
        /// such as delivery delay or application properties.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="producerOptions">Additional options for message delivery.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task ProduceAsync(
            TMessage message,
            MessageOptions producerOptions,
            CancellationToken cancellationToken);

        /// <summary>
        /// Sends a message to the Azure Service Bus with a specified delivery delay.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="delay">The delay duration before the message becomes available.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task ProduceAsync(
            TMessage message,
            TimeSpan delay,
            CancellationToken cancellationToken);

        /// <summary>
        /// Sends a message to the Azure Service Bus with application properties
        /// that can be used for downstream filtering or routing.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="applicationProperties">Key-value pairs attached to the message.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task ProduceAsync(
            TMessage message,
            IDictionary<string, string> applicationProperties,
            CancellationToken cancellationToken);
    }
}

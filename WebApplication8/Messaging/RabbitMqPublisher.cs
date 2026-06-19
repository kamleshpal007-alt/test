using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace WebApplication8.Messaging
{
    // The PRODUCER: serializes an object to JSON and drops it onto the queue.
    // Controllers call PublishAsync(...) to "fire and forget" an event.
    public class RabbitMqPublisher
    {
        private readonly RabbitMqConnection _connection;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(RabbitMqConnection connection, ILogger<RabbitMqPublisher> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task PublishAsync(object message, CancellationToken ct = default)
        {
            try
            {
                // A channel is cheap; open one per publish so we never share it
                // across concurrent requests (channels are not thread-safe).
                await using var channel = await _connection.CreateChannelAsync(ct);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: "",                              // default exchange
                    routingKey: RabbitMqConnection.QueueName,  // -> goes straight to our queue
                    body: body,
                    cancellationToken: ct);

                _logger.LogInformation("📤 Published: {Json}", json);
            }
            catch (Exception ex)
            {
                // A messaging failure must NOT break the HTTP request — just log it.
                _logger.LogWarning(ex, "⚠️ Failed to publish RabbitMQ message (is the broker running?)");
            }
        }
    }
}

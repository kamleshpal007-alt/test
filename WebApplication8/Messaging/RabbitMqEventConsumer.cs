using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApplication8.Messaging
{
    // Base CONSUMER: a long-running background service (starts with the app and
    // keeps listening). Every message that lands on its queue is stored under a
    // topic so the matching web page can display it. Each module subclasses this
    // with its own queue + topic.
    public abstract class RabbitMqEventConsumer : BackgroundService
    {
        private readonly RabbitMqConnection _connection;
        private readonly ILogger _logger;
        private readonly EventStore _store;
        private readonly string _queueName;
        private readonly string _topic;
        private IChannel? _channel;

        protected RabbitMqEventConsumer(
            RabbitMqConnection connection, ILogger logger, EventStore store, string queueName, string topic)
        {
            _connection = connection;
            _logger = logger;
            _store = store;
            _queueName = queueName;
            _topic = topic;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _channel = await _connection.CreateChannelAsync(_queueName, stoppingToken);

                // AsyncEventingBasicConsumer raises ReceivedAsync for each message.
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation("📥 Consumed {Topic} event: {Json}", _topic, json);

                    // Save it (under this consumer's topic) so the web page can display it.
                    _store.Add(_topic, json);
                    await Task.CompletedTask;
                };

                // autoAck:true = tell the broker "got it" automatically once delivered.
                await _channel.BasicConsumeAsync(
                    queue: _queueName,
                    autoAck: true,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("👂 Consumer listening on queue '{Queue}'", _queueName);
            }
            catch (Exception ex)
            {
                // Don't crash the whole app if the broker is down — just warn.
                _logger.LogWarning(ex, "⚠️ RabbitMQ consumer could not start (is the broker running?)");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
                await _channel.DisposeAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}

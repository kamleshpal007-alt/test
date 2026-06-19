using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApplication8.Messaging
{
    // The CONSUMER: a long-running background service (starts with the app and
    // keeps listening). Every message that lands on the queue triggers our handler.
    // In a real app this is where you'd send an email, write an audit log, etc.
    public class EmployeeEventConsumer : BackgroundService
    {
        private readonly RabbitMqConnection _connection;
        private readonly ILogger<EmployeeEventConsumer> _logger;
        private readonly EventStore _store;
        private IChannel? _channel;

        public EmployeeEventConsumer(RabbitMqConnection connection, ILogger<EmployeeEventConsumer> logger, EventStore store)
        {
            _connection = connection;
            _logger = logger;
            _store = store;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _channel = await _connection.CreateChannelAsync(stoppingToken);

                // AsyncEventingBasicConsumer raises ReceivedAsync for each message.
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation("📥 Consumed employee event: {Json}", json);

                    // Save it so the web page can display it.
                    _store.Add(json);
                    await Task.CompletedTask;
                };

                // autoAck:true = tell the broker "got it" automatically once delivered.
                await _channel.BasicConsumeAsync(
                    queue: RabbitMqConnection.QueueName,
                    autoAck: true,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("👂 Consumer listening on queue '{Queue}'", RabbitMqConnection.QueueName);
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

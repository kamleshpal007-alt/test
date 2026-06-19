using RabbitMQ.Client;

namespace WebApplication8.Messaging
{
    // Owns ONE shared connection to the RabbitMQ broker (connections are expensive,
    // so we open a single one and create cheap "channels" on top of it as needed).
    // Registered as a singleton so the whole app reuses the same connection.
    public sealed class RabbitMqConnection : IAsyncDisposable
    {
        // The queue both the publisher and consumer use.
        public const string QueueName = "employee-events";

        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly SemaphoreSlim _lock = new(1, 1); // guards lazy connect
        private IConnection? _connection;

        public RabbitMqConnection(IConfiguration config, ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory
            {
                HostName = config["RabbitMq:Host"] ?? "localhost",
                UserName = config["RabbitMq:User"] ?? "guest",
                Password = config["RabbitMq:Password"] ?? "guest",
            };
        }

        // Opens a channel on the shared connection and makes sure the queue exists.
        // QueueDeclare is idempotent — safe to call every time.
        public async Task<IChannel> CreateChannelAsync(CancellationToken ct = default)
        {
            await EnsureConnectionAsync(ct);

            var channel = await _connection!.CreateChannelAsync(cancellationToken: ct);
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,      // queue survives a broker restart
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            return channel;
        }

        // Creates the connection once, the first time it's needed (thread-safe).
        private async Task EnsureConnectionAsync(CancellationToken ct)
        {
            if (_connection is { IsOpen: true }) return;

            await _lock.WaitAsync(ct);
            try
            {
                if (_connection is { IsOpen: true }) return;
                _connection = await _factory.CreateConnectionAsync(ct);
                _logger.LogInformation("✅ Connected to RabbitMQ at {Host}", _factory.HostName);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
            _lock.Dispose();
        }
    }
}

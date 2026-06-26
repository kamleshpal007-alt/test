namespace WebApplication8.Messaging
{
    public class SalaryEventConsumer : RabbitMqEventConsumer
    {
        public SalaryEventConsumer(
            RabbitMqConnection connection, ILogger<SalaryEventConsumer> logger, EventStore store)
            : base(connection, logger, store, RabbitMqConnection.SalaryQueue, "salary")
        {
        }
    }
}

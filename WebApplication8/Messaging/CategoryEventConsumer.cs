namespace WebApplication8.Messaging
{
    // Consumes category events off the "category-events" queue and stores them
    // under the "category" topic for live UI display.
    public class CategoryEventConsumer : RabbitMqEventConsumer
    {
        public CategoryEventConsumer(
            RabbitMqConnection connection,
            ILogger<CategoryEventConsumer> logger,
            EventStore store)
            : base(connection, logger, store, RabbitMqConnection.CategoryQueue, "category")
        {
        }
    }
}

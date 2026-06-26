namespace WebApplication8.Messaging
{
    // Consumes department events off the "department-events" queue and stores them
    // under the "department" topic. All the work lives in the shared base class.
    public class DepartmentEventConsumer : RabbitMqEventConsumer
    {
        public DepartmentEventConsumer(
            RabbitMqConnection connection, ILogger<DepartmentEventConsumer> logger, EventStore store)
            : base(connection, logger, store, RabbitMqConnection.DepartmentQueue, "department")
        {
        }
    }
}

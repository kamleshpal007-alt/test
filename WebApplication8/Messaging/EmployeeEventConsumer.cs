namespace WebApplication8.Messaging
{
    // Consumes employee events off the "employee-events" queue and stores them
    // under the "employee" topic. All the work lives in the shared base class.
    public class EmployeeEventConsumer : RabbitMqEventConsumer
    {
        public EmployeeEventConsumer(
            RabbitMqConnection connection, ILogger<EmployeeEventConsumer> logger, EventStore store)
            : base(connection, logger, store, RabbitMqConnection.EmployeeQueue, "employee")
        {
        }
    }
}

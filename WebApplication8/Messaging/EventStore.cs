namespace WebApplication8.Messaging
{
    // Keeps the most recent consumed RabbitMQ events in memory so a web page
    // can display them (much easier to see than terminal logs).
    public class EventStore
    {
        private readonly LinkedList<EventEntry> _events = new();
        private readonly object _lock = new();
        private const int MaxItems = 20;

        public void Add(string message)
        {
            lock (_lock)
            {
                _events.AddFirst(new EventEntry(DateTime.Now.ToString("HH:mm:ss"), message));
                while (_events.Count > MaxItems)
                    _events.RemoveLast();
            }
        }

        public IEnumerable<EventEntry> GetRecent()
        {
            lock (_lock) return _events.ToList();
        }
    }

    public record EventEntry(string Time, string Message);
}

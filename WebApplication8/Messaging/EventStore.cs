namespace WebApplication8.Messaging
{
    // Keeps the most recent consumed RabbitMQ events in memory so a web page
    // can display them (much easier to see than terminal logs). Events are grouped
    // by topic (e.g. "employee", "department") so each module shows only its own.
    public class EventStore
    {
        private readonly Dictionary<string, LinkedList<EventEntry>> _topics = new();
        private readonly object _lock = new();
        private const int MaxItems = 20;

        public void Add(string topic, string message)
        {
            lock (_lock)
            {
                if (!_topics.TryGetValue(topic, out var events))
                    _topics[topic] = events = new LinkedList<EventEntry>();

                events.AddFirst(new EventEntry(DateTime.Now.ToString("HH:mm:ss"), message));
                while (events.Count > MaxItems)
                    events.RemoveLast();
            }
        }

        public IEnumerable<EventEntry> GetRecent(string topic)
        {
            lock (_lock)
                return _topics.TryGetValue(topic, out var events)
                    ? events.ToList()
                    : new List<EventEntry>();
        }
    }

    public record EventEntry(string Time, string Message);
}

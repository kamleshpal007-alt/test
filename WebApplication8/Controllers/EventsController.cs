using Microsoft.AspNetCore.Mvc;
using WebApplication8.Messaging;

namespace WebApplication8.Controllers
{
   
    // Returns the most recent RabbitMQ events the consumer has processed,
    // so the web page can display them live.
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly EventStore _store;

        public EventsController(EventStore store) => _store = store;

        // GET /api/events?topic=employee   (defaults to employee for back-compat)
        [HttpGet]
        public IEnumerable<EventEntry> Get(string topic = "employee") => _store.GetRecent(topic);
    }
}

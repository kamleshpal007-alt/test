using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Messaging;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    [ApiController]
    [Route("api/salaries")]
    public class SalariesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly RabbitMqPublisher _publisher;

        public SalariesController(AppDbContext db, RabbitMqPublisher publisher)
        {
            _db = db;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<object> GetAll(int page = 1, int pageSize = 10)
        {
            var query = _db.Salaries.AsNoTracking().OrderByDescending(s => s.EffectiveDate);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new { items, total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) };
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Salary>> GetById(int id)
        {
            var s = await _db.Salaries.FindAsync(id);
            return s is null ? NotFound() : s;
        }

        [HttpPost]
        public async Task<ActionResult<Salary>> Create([FromBody] Salary input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            input.Id = 0;
            _db.Salaries.Add(input);
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.SalaryQueue, new { Event = "SalaryCreated", input.Id, input.EmployeeId });
            return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Salary>> Update(int id, [FromBody] Salary input)
        {
            var s = await _db.Salaries.FindAsync(id);
            if (s is null) return NotFound();
            s.Amount = input.Amount;
            s.Currency = input.Currency;
            s.EffectiveDate = input.EffectiveDate;
            s.Note = input.Note;
            s.EmployeeId = input.EmployeeId;
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.SalaryQueue, new { Event = "SalaryUpdated", s.Id, s.EmployeeId });
            return s;
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Salaries.FindAsync(id);
            if (s is null) return NotFound();
            _db.Salaries.Remove(s);
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.SalaryQueue, new { Event = "SalaryDeleted", id });
            return NoContent();
        }
    }
}

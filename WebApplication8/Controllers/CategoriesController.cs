using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Messaging;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly RabbitMqPublisher _publisher;

        public CategoriesController(AppDbContext db, RabbitMqPublisher publisher)
        {
            _db = db;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<object> GetAll(int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var query = _db.Categories.AsNoTracking().OrderBy(c => c.Id);
            var total = await query.CountAsync();

            var totaldata = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                items,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            return category is null ? NotFound() : category;
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create([FromBody] Category input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _db.Categories.AnyAsync(c => c.Code == input.Code))
                return Conflict($"A category with code '{input.Code}' already exists.");

            input.Id = 0;
            _db.Categories.Add(input);
            await _db.SaveChangesAsync();

            await _publisher.PublishAsync(RabbitMqConnection.CategoryQueue, new { Event = "CategoryCreated", input.Id, input.Name, input.Code });
            return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Category>> Update(int id, [FromBody] Category input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _db.Categories.FindAsync(id);
            if (category is null) return NotFound();

            if (await _db.Categories.AnyAsync(c => c.Code == input.Code && c.Id != id))
                return Conflict($"A category with code '{input.Code}' already exists.");

            category.Name = input.Name;
            category.Code = input.Code;
            category.Description = input.Description;

            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.CategoryQueue, new { Event = "CategoryUpdated", category.Id, category.Name, category.Code });
            return category;
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category is null) return NotFound();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.CategoryQueue, new { Event = "CategoryDeleted", id });
            return NoContent();
        }
    }
}

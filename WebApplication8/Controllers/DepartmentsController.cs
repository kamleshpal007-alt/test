using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Messaging;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    // REST API for the Departments module — backed by a real database via EF Core.
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly RabbitMqPublisher _publisher;

        // The DbContext and the RabbitMQ publisher are injected by DI.
        public DepartmentsController(AppDbContext db, RabbitMqPublisher publisher)
        {
            _db = db;
            _publisher = publisher;
        }

        // READ a PAGE  ->  GET /api/departments?page=1&pageSize=5
        // Server-side pagination: only the requested page is read from the DB,
        // plus the total count so the UI can build page buttons.
        [HttpGet]
        public async Task<object> GetAll(int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var query = _db.Departments.AsNoTracking().OrderBy(d => d.Id);

            var total = await query.CountAsync();                 // total rows in DB
            var items = await query
                .Skip((page - 1) * pageSize)                      // jump over previous pages
                .Take(pageSize)                                   // take just this page
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

        // READ one  ->  GET /api/departments/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Department>> GetById(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            return department is null ? NotFound() : department;
        }

        // CREATE  ->  POST /api/departments
        [HttpPost]
        public async Task<ActionResult<Department>> Create([FromBody] Department input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Enforce the unique-code rule with a friendly message before hitting
            // the database's unique index (which would otherwise throw).
            if (await _db.Departments.AnyAsync(d => d.Code == input.Code))
                return Conflict($"A department with code '{input.Code}' already exists.");

            input.Id = 0; // let the database assign the id
            _db.Departments.Add(input);
            await _db.SaveChangesAsync();

            // Announce the event to RabbitMQ (the consumer will pick it up).
            await _publisher.PublishAsync(RabbitMqConnection.DepartmentQueue, new { Event = "DepartmentCreated", input.Id, input.Name, input.Code });

            return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
        }

        // UPDATE  ->  PUT /api/departments/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Department>> Update(int id, [FromBody] Department input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var department = await _db.Departments.FindAsync(id);
            if (department is null) return NotFound();

            // Block renaming a department's code to one already used by another row.
            if (await _db.Departments.AnyAsync(d => d.Code == input.Code && d.Id != id))
                return Conflict($"A department with code '{input.Code}' already exists.");

            department.Name = input.Name;
            department.Code = input.Code;
            department.Description = input.Description;

            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.DepartmentQueue, new { Event = "DepartmentUpdated", department.Id, department.Name, department.Code });
            return department;
        }

        // DELETE  ->  DELETE /api/departments/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department is null) return NotFound();

            _db.Departments.Remove(department);
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.DepartmentQueue, new { Event = "DepartmentDeleted", id });
            return NoContent();
        }
    }
}

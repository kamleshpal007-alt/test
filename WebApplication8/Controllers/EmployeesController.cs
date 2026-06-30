using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using WebApplication8.Data;
using WebApplication8.Messaging;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    // REST API for the Employees module — backed by a real database via EF Core.
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly RabbitMqPublisher _publisher;
        private readonly RabbitMQTracingOptions _rabbitMqPublisher;

        // The DbContext and the RabbitMQ publisher are injected by DI.
        public EmployeesController(AppDbContext db, RabbitMqPublisher publisher,RabbitMQTracingOptions objrabbitmqtracingOption)
        {
            _db = db;
            _publisher = publisher;
            _rabbitMqPublisher = objrabbitmqtracingOption;
        }

        // READ a PAGE  ->  GET /api/employees?page=1&pageSize=5
        // Server-side pagination: only the requested page is read from the DB,
        // plus the total count so the UI can build page buttons.
        [HttpGet]
        public async Task<object> GetAll(int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var query = _db.Employees.AsNoTracking().OrderBy(e => e.Id);

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

        // READ one  ->  GET /api/employees/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Employee>> GetById(int id)
        {

            var employee = await _db.Employees.FindAsync(id);
            return employee is null ? NotFound() : employee;
        }

        // CREATE  ->  POST /api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> Create([FromBody] Employee input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            input.Id = 0; // let the database assign the id
            _db.Employees.Add(input);
            await _db.SaveChangesAsync();

            // Announce the event to RabbitMQ (the consumer will pick it up).
            await _publisher.PublishAsync(RabbitMqConnection.EmployeeQueue, new { Event = "EmployeeCreated", input.Id, input.Name });

            return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
        }

        // UPDATE  ->  PUT /api/employees/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Employee>> Update(int id, [FromBody] Employee input)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee is null) return NotFound();
                
            employee.Name = input.Name;
            employee.Email = input.Email;
            employee.Department = input.Department;
            employee.Salary = input.Salary;

            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.EmployeeQueue, new { Event = "EmployeeUpdated", employee.Id, employee.Name });
            return employee;
        }

        // DELETE  ->  DELETE /api/employees/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee is null) return NotFound();

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync(RabbitMqConnection.EmployeeQueue, new { Event = "EmployeeDeleted", id });
            return NoContent();
        }
    }
}

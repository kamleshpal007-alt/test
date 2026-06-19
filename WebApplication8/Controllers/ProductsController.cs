using Microsoft.AspNetCore.Mvc;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    // REST API for the React CRUD demo.
    // [ApiController] makes the actions return JSON and auto-validate input.
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {

     
        // In-memory store. Static so it survives between requests (demo only —
        // a real app would use a database via Entity Framework Core).
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Keyboard", Price = 1200 },
            new Product { Id = 2, Name = "Mouse",    Price = 600  },
            new Product { Id = 3, Name = "Monitor",  Price = 9500 },
        };
        private static int _nextId = 4;
        private static readonly object _lock = new();

        // READ all  ->  GET /api/products
        [HttpGet]
        public IEnumerable<Product> GetAll()
        {
            lock (_lock) return _products.ToList();
        }

        // READ one  ->  GET /api/products/5
        [HttpGet("{id:int}")]
        public ActionResult<Product> GetById(int id)
        {
            lock (_lock)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                return product is null ? NotFound() : product;
            }
        }

        // CREATE  ->  POST /api/products   (body: { name, price })
        [HttpPost]
        public ActionResult<Product> Create([FromBody] Product input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return BadRequest("Name is required.");

            lock (_lock)
            {
                input.Id = _nextId++;
                _products.Add(input);
                // 201 Created + the new object (with its server-assigned Id)
                return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
            }
        }

        // UPDATE  ->  PUT /api/products/5   (body: { name, price })
        [HttpPut("{id:int}")]
        public ActionResult<Product> Update(int id, [FromBody] Product input)
        {
            lock (_lock)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product is null) return NotFound();

                product.Name = input.Name;
                product.Price = input.Price;
                return product;
            }
        }

        // DELETE  ->  DELETE /api/products/5
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            lock (_lock)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product is null) return NotFound();

                _products.Remove(product);
                return NoContent(); // 204 = success, nothing to return
            }
        }
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Renders the Razor page that hosts the React app.
        public IActionResult React()
        {
            return View();
        }

        // Renders the Razor page that hosts the React CRUD demo.
        public IActionResult Products()
        {
            return View();
        }

        // Renders the Razor page that hosts the database-backed Employees module.
        public IActionResult Employees()
        {
            return View();
        }

        // JSON API the React component fetches on load.
        [HttpGet("api/tasks")]
        public IActionResult GetTasks()
        {
            var tasks = new[]
            {
                new { id = 1, label = "Learn React state", done = true },
                new { id = 2, label = "Call the ASP.NET API", done = false },
                new { id = 3, label = "Render a component", done = false },
            };
            return Json(tasks);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

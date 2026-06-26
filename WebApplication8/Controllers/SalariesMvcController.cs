using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    public class SalariesMvcController : Controller
    {
        private readonly AppDbContext _db;

        public SalariesMvcController(AppDbContext db)
        {
            _db = db;
        }

        private async Task PopulateEmployeesDropDownAsync()
        {
            var employees = await _db.Employees.OrderBy(e => e.Name)
                .Select(e => new SelectListItem(e.Name, e.Id.ToString()))
                .ToListAsync();
            ViewBag.EmployeeList = employees;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _db.Salaries.Include(s => s.Employee).OrderByDescending(s => s.EffectiveDate).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateEmployeesDropDownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Salary model)
        {
            if (!await EmployeeExists(model.EmployeeId))
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Selected employee does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateEmployeesDropDownAsync();
                return View(model);
            }

            _db.Salaries.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = await _db.Salaries.FindAsync(id);
            if (s is null) return NotFound();
            await PopulateEmployeesDropDownAsync();
            return View(s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Salary model)
        {
            if (id != model.Id) return BadRequest();

            if (!await EmployeeExists(model.EmployeeId))
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Selected employee does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateEmployeesDropDownAsync();
                return View(model);
            }

            _db.Salaries.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var s = await _db.Salaries.Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == id);
            if (s is null) return NotFound();
            return View(s);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Salaries.Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == id);
            if (s is null) return NotFound();
            return View(s);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var s = await _db.Salaries.FindAsync(id);
            if (s is null) return NotFound();
            _db.Salaries.Remove(s);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> EmployeeExists(int employeeId)
        {
            return await _db.Employees.AnyAsync(e => e.Id == employeeId);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WebApplication8.Models;

namespace WebApplication8.Data
{
    // The DbContext is EF Core's main class: it represents a session with the
    // database and exposes each table as a DbSet<T>.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // This becomes the "Employees" table in the database.
        public DbSet<Employee> Employees => Set<Employee>();

    }
}

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

        // This becomes the "Departments" table in the database.
        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Category> Categories { get; set; }

        // Salaries table
        public DbSet<WebApplication8.Models.Salary> Salaries => Set<WebApplication8.Models.Salary>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Department codes are unique (e.g. you can't have two "HR" departments).
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            // Category codes are unique too.
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Seed employees (matches the data inserted by the InitialCreate migration).
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, Name = "Asha Patel", Email = "asha@company.com", Department = "Engineering", Salary = 65000m },
                new Employee { Id = 2, Name = "Rohan Mehta", Email = "rohan@company.com", Department = "Sales", Salary = 48000m },
                new Employee { Id = 3, Name = "Priya Nair", Email = "priya@company.com", Department = "HR", Salary = 52000m }
            );
        }
    }
}

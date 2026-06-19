using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    // An Employee record stored in the database.
    public class Employee
    {
        // Primary key. EF Core treats a property named "Id" as the key by default.
        public int Id {     get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = "";

        [MaxLength(100)]
        public string Department { get; set; } = "";

        public decimal Salary { get; set; }
    }
}

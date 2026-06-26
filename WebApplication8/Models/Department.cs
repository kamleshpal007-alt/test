using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    // A Department record stored in the database.
    public class Department
    {
        // Primary key. EF Core treats a property named "Id" as the key by default.
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        // A short unique code for the department, e.g. "HR", "ENG".
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = "";

        [MaxLength(250)]
        public string Description { get; set; } = "";
    }
}

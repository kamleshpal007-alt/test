using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    // A Category record stored in the database.
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = "";

        [MaxLength(250)]
        public string Description { get; set; } = "";
    }
}

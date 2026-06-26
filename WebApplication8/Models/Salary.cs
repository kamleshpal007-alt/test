using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication8.Models
{
    public class Salary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(8)]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime EffectiveDate { get; set; }

        public string? Note { get; set; }

        // Navigation property
        public Employee? Employee { get; set; }
    }
}

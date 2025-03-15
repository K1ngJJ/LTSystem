using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class BillHistory : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ActionTaken { get; set; } // Example: "Bill passed committee review"

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [ForeignKey("Bill")]
        public int BillId { get; set; }
        public Bill Bill { get; set; }

        // ✅ Add the missing properties below
        [Required]
        public string Status { get; set; }  // Fix for CS1061

        public string Description { get; set; } // Fix for CS0117

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow; // Fix for CS0117
    }
}

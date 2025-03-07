using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class BillHistory : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillId { get; set; }
        [ForeignKey("BillId")]
        public Bill Bill { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } // Added Status field
    }
}

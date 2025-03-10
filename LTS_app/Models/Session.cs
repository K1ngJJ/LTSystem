using System.ComponentModel.DataAnnotations;

namespace LTS_app.Models
{
    public class Session : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Example: "2025 Legislative Session"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}

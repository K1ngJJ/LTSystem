using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class CitizenFeedback : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Bill")]
        public int BillId { get; set; }
        public Bill Bill { get; set; }
    }
}

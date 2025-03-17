using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class CitizenFeedback : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillId { get; set; }
        [ForeignKey("BillId")]
        public Bill Bill { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FeedbackText { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}

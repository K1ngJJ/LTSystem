using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class Bill : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime IntroducedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, In Committee, Voted, Passed, Rejected

        [Required]
        [ForeignKey("Legislator")]
        public int LegislatorId { get; set; }
        public Legislator Legislator { get; set; }

        [Required]
        [ForeignKey("Committee")]
        public int CommitteeId { get; set; }
        public Committee Committee { get; set; }

        [Required]
        [ForeignKey("Session")]
        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        public ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<BillHistory> BillHistories { get; set; } = new List<BillHistory>();
        public ICollection<CitizenFeedback> CitizenFeedbacks { get; set; } = new List<CitizenFeedback>();

    }
}

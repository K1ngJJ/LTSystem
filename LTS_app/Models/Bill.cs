using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class Bill : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        // Legislator Relationship
        [Required]
        public int LegislatorId { get; set; }

        [ForeignKey(nameof(LegislatorId))]  // Ensure EF Core links this correctly
        public Legislator Legislator { get; set; }

        // Committee Relationship
        [Required]
        public int CommitteeId { get; set; }

        [ForeignKey(nameof(CommitteeId))]  // Ensure EF Core links this correctly
        public Committee Committee { get; set; }

        // Related Collections
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
        public ICollection<BillHistory> BillHistories { get; set; } = new List<BillHistory>();
        public ICollection<CitizenFeedback> CitizenFeedbacks { get; set; } = new List<CitizenFeedback>();
    }




}

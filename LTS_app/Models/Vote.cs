using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class Vote : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Choice { get; set; } // YES, NO, ABSTAIN

        [Required]
        public DateTime DateVoted { get; set; } = DateTime.UtcNow;

        [ForeignKey("Bill")]
        public int BillId { get; set; }
        public Bill Bill { get; set; }

        [ForeignKey("Legislator")]
        public int LegislatorId { get; set; }
        public Legislator Legislator { get; set; }
    }
}

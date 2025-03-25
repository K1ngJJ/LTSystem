using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LTS_app.Models
{
    public class CommitteeLegislator : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Committee")]
        public int CommitteeId { get; set; }
        public Committee Committee { get; set; }

        [ForeignKey("Legislator")]
        public int LegislatorId { get; set; }
        public Legislator Legislator { get; set; }
        public bool IsAdmin { get; set; } // ✅ New Column
    }

}

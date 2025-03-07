using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LTS_app.Models
{
    public class Vote : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillId { get; set; }
        [ForeignKey("BillId")]
        public Bill Bill { get; set; }

        [Required]
        public int LegislatorId { get; set; }
        [ForeignKey("LegislatorId")]
        public Legislator Legislator { get; set; }

        [Required]
        public string VoteType { get; set; }

        [Required]
        public DateTime VoteDate { get; set; }
    }


}

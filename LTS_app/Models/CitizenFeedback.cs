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
        public string Comment { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; }

    }


}

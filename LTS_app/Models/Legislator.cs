using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class Legislator : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Party { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string District { get; set; }

        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    }

}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class Legislator
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; } // Navigation Property

        [Required]
        public string Position { get; set; } // Example: "Senator", "Representative"

        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<Committee> Committees { get; set; } = new List<Committee>();

        // Ensure only users with role "Legislator" are linked here (Logic applied in queries)
        [NotMapped]
        public bool IsLegislator => User?.Role == "Legislator";
    }
}

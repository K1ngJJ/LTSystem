using System;
using System.Collections.Generic;
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
        public DateTime IntroducedDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, In Committee, Voted, Passed, Rejected

        // ✅ Direct Foreign Key to User
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        // ✅ Make CommitteeId nullable
        [ForeignKey("Committee")]
        public int? CommitteeId { get; set; }  // <-- Changed to int?
        public Committee? Committee { get; set; }  // Nullable reference

        [Required]
        [ForeignKey("Session")]
        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        public ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<BillHistory> BillHistories { get; set; } = new List<BillHistory>();
        public ICollection<UserFeedback> CitizenFeedbacks { get; set; } = new List<UserFeedback>();
    }
}

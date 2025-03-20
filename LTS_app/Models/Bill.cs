using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace LTS_app.Models
{
    public class Bill : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(255)] // Limits title length
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)] // Prevents excessive text
        public string? Description { get; set; }

        [Required]
        public DateTime IntroducedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, In Committee, Voted, Passed, Rejected

        // ✅ Foreign Key to User (Legislator)
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // ✅ Foreign Key to Committee (Optional)
        [ForeignKey("Committee")]
        public int? CommitteeId { get; set; } 
        public virtual Committee? Committee { get; set; }  

        // ✅ Foreign Key to Session (Required)
        [Required]
        [ForeignKey("Session")]
        public int SessionId { get; set; }
        public virtual Session Session { get; set; } = null!;

        // ✅ File Uploads (PDF, DOCX)
        [StringLength(255)]
        public string? FilePath { get; set; }  

        // ✅ Image Uploads (JPG, PNG)
        [StringLength(255)]
        public string? ImagePath { get; set; } 

        // ✅ Video Uploads (MP4, MOV, AVI)
        [StringLength(255)]
        public string? VideoPath { get; set; }

        // ✅ Related Data
        public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<BillHistory> BillHistories { get; set; } = new List<BillHistory>();
        public virtual ICollection<UserFeedback> UserFeedbacks { get; set; } = new List<UserFeedback>();

        // ✅ NotMapped Properties (For Form Handling)
        [NotMapped]
        public IFormFile? File { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public IFormFile? VideoFile { get; set; }
    }
}

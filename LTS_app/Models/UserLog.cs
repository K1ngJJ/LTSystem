using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class UserLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string FullName { get; set; } // 🔹 Store Full Name

        [Required]
        [StringLength(255)]
        public string Action { get; set; } // Example: "Login", "Logout", "Created Bill"

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string IPAddress { get; set; }

        // Relationship
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}

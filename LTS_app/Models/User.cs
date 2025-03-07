using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTS_app.Models
{
    public class User : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        public string Token { get; set; } // Store authentication token

        public bool IsActive { get; set; } = true; // Enable/Disable feature

        public bool IsConfirmed { get; set; } = false; // Email verification status

        public string? ConfirmationToken { get; set; } // Unique token for verification

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

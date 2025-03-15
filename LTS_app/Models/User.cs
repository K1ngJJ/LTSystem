using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

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

        public string FullName { get; set; }

        public string Token { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsConfirmed { get; set; } = false;

        public string? ConfirmationToken { get; set; }

        public string? ResetPasswordToken { get; set; }  // Token for resetting password

        public DateTime? ResetPasswordExpiry { get; set; } // Expiry time for reset token

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();

        public Legislator Legislator { get; set; }
    }

}

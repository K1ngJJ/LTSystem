﻿using System;
using System.ComponentModel.DataAnnotations;

namespace LTS_app.Models
{
    public abstract class BaseEntity
    {
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Default to now

        public DateTime? UpdatedAt { get; set; }
    }
}

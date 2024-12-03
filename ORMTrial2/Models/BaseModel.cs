using System;
using System.ComponentModel.DataAnnotations;

namespace ORMTrial2.Models
{
    public abstract class BaseModel
    {
        [Key]
        public int Id { get; set; } // Primary key for all models

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Auto-set timestamp for creation

    }
}

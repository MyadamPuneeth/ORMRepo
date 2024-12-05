using System;
using System.ComponentModel.DataAnnotations;

namespace ORMTrial2.Models
{
    public abstract class BaseModel
    {
        [Key]
        public int Id { get; set; } // Primary key for all models

    }
}

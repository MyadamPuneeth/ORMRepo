using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ORMTrial2.Models
{
    [Table("Student")]
    public class Student
    {
        [Key]
        public int StuId { get; set; }
        public int rollNUmber { get; set; }
        public string stuname { get; set; }
        
        [ForeignKey("User")]
        public int userId { get; set; }
    }
}

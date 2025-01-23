using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ORMTrial2.Models
{
[Table("User")]
public class User
{
public string UserName { get; set; }
public int Age { get; set; }
[Key]
public int Id { get; set; }
    }
}

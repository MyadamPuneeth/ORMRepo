using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ORMTrial2.Models
{
    public class User : BaseModel
    {
        public string UserName { get; set; }
        public int Age { get; set; }

    }
}

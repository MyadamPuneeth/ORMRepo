using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTrial2.Models
{
    public class User : BaseModel
    {
        public string UserName { get; set; }
        public int Age { get; set; }
    }
}

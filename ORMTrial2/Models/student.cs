using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTrial2.Models
{
    public class student : BaseModel
    {
        public int rollNUmber { get; set; }
        public string stuname { get; set; }
    }
}

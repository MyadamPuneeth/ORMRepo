using ORMTrial2.Models;
using ORMTrial2.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTrial2
{
    public class AppDbContext : DbFrame
    {
        public DbFrame<User> user { get; set; }
        public DbFrame<student> Student { get; set; }
        public DbFrame<Vattikuti> Vattikuti { get; set; }
    }
}

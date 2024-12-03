using ORMTrial2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMTrial2
{
    public class AppDbContext : DBFrame
    {
        public DbSet<User> user { get; set; }
        public DbSet<student> Student { get; set; }
        public DbSet<Vattikuti> Vattikuti { get; set; }
    }
}

using BuildManager.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEF
{
    public class BuildManagerContext : DbContext
    {
        public DbSet<TestResult> TestResults { get; set; }
    }
}

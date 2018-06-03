using BuildManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEF
{
    public class TestResultRepository
    {
        public void Save(TestResult testResult)
        {
            using(BuildManagerContext context = new BuildManagerContext())
            {
                context.TestResults.Add(testResult);
                context.SaveChanges();
            }
        }


    }
}

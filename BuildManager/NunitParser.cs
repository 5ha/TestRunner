using BuildManager.Model;
using MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public class NunitParser
    {
        public TestExecutionResult Parse(TestResult test)
        {
            TestExecutionResult res = new TestExecutionResult
            {
                DateUtc = DateTime.UtcNow,
                FullName = test.FullName

            };

            return res;
        }
    }
}

using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InContainerShared
{
    public class StringListener : ITestEventListener
    {
        public StringBuilder TestResults = new StringBuilder();

        public void OnTestEvent(string report)
        {
            TestResults.Append(report);
        }
    }
}

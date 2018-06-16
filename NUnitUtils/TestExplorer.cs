using MessageModels;
using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NUnitUtils
{
    public class TestExplorer : IDisposable
    {
        ITestEngine _testEngine;

        public TestExplorer()
        {
            _testEngine = TestEngineActivator.CreateInstance();
        }

        public void Dispose()
        {
            if (_testEngine != null) _testEngine.Dispose();
        }

        public List<RunTest> GetTests(string build, string pathToExplore)
        {
            List<RunTest> res = new List<RunTest>();
            var files = Directory.GetFiles(pathToExplore, "*.dll", SearchOption.AllDirectories);
            TestPackage package = new TestPackage(files);

            using (ITestRunner runner = _testEngine.GetRunner(package))
            {
                var testSuites = runner.Explore(TestFilter.Empty);
                var testCases = testSuites.SelectNodes("//test-case");

                foreach (XmlNode n in testCases)
                {
                    RunTest message = new RunTest
                    {
                        Build = build,
                        FullName = n.Attributes["fullname"].Value
                    };
                    res.Add(message);
                }
            }
            return res;
        }
    }
}

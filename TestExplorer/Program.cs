using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TestExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ITestEngine testEngine = TestEngineActivator.CreateInstance())
            {
                var files = Directory.GetFiles(ConfigurationManager.AppSettings["directoryToSearch"], "*.dll", SearchOption.AllDirectories);
                TestPackage package = new TestPackage(files);

                using (ITestRunner runner = testEngine.GetRunner(package))
                {
                    var testSuites = runner.Explore(TestFilter.Empty);
                    var testCases = testSuites.SelectNodes("//test-case");

                    foreach (XmlNode n in testCases)
                    {
                        Console.WriteLine(n.Attributes["fullname"].Value);
                    }
                }
            }
        }
    }
}

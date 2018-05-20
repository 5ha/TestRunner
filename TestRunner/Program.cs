using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModels;
using NUnit.Engine;
using System.Xml;
using System.IO;

namespace TestRunner
{
    class Program 
    {
        private static TransportService.TestClientHelper _helper;

        static void Main(string[] args)
        {
            _helper
                = new TransportService.TestClientHelper(
                    "test_requests", 
                    "Blue", 
                    "test_responses", 
                    "test_responses",
                    ShutDown);

            string baseDirectory = @"C:\\Users\\shawn\\source\\repos\\TestNUnitRunner\\SystemUnderTest\\bin\\Debug\\";

            var files = Directory.GetFiles(baseDirectory, "*.dll", SearchOption.AllDirectories).ToList();

            TestExecutor executor = new TestExecutor(files);

            _helper.Subscribe<RunTest>((m) =>
            {
                Console.WriteLine($"Running {m.FullName} ...");
                var responseXML = executor.Execute(m);

                var responseNode = responseXML.SelectSingleNode("//test-case");
                var testResult = responseNode.Attributes["result"].Value;
                Console.WriteLine($"{m.FullName} : {testResult.ToUpper()}");

                _helper.SendTestResult(new TestResult { Result = responseXML });
            });

            Console.WriteLine("Listening ...");
        }

        private static void ShutDown()
        {
            Console.WriteLine("Environment exiting.");
            _helper.Dispose();
            Environment.Exit(0);
        }
    }

    public class Listener : ITestEventListener
    {
        public void OnTestEvent(string report)
        {
            var x = report;
        }
    }
}

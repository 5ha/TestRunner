using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModels;
using NUnit.Engine;
using System.Xml;


namespace TestRunner
{
    class Program 
    {
        static void Main(string[] args)
        {

            TransportService.Helper helper = new TransportService.Helper("Blue");

            List<string> testAssemblies = new List<string>()
            {
                @"C:\\Users\\shawn\\source\\repos\\TestNUnitRunner\\SystemUnderTest\\bin\\Debug\\SystemUnderTest.dll"
            };
            TestExecutor executor = new TestExecutor(testAssemblies);

            helper.Subscribe<RunTest>((m) =>
            {
                Console.WriteLine($"Running {m.FullName} ...");
                var responseXML = executor.Execute(m);

                var responseNode = responseXML.SelectSingleNode("//test-case");
                var testResult = responseNode.Attributes["result"].Value;
                Console.WriteLine($"{m.FullName} : {testResult.ToUpper()}");
            }, () => {
                Console.WriteLine("Environment exiting.");
                helper.Dispose();
                Environment.Exit(0);
            });

            Console.WriteLine("Listening ...");
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

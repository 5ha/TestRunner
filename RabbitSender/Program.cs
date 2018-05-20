using MessageModels;
using NUnit.Engine;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace RabbitSender
{
    class Program
    {
        static void Main(string[] args)
        {

            TransportService.Helper helperBlue = new TransportService.Helper("Blue");

            ITestEngine testEngine = TestEngineActivator.CreateInstance();

            string baseDirectory = @"C:\\Users\\shawn\\source\\repos\\TestNUnitRunner\\SystemUnderTest\\bin\\Debug\\";

            var files = Directory.GetFiles(baseDirectory, "*.dll", SearchOption.AllDirectories);

            TestPackage package = new TestPackage(files);

            ITestRunner runner = testEngine.GetRunner(package);

            var testSuites = runner.Explore(TestFilter.Empty);

            var testCases = testSuites.SelectNodes("//test-case");

            foreach (XmlNode n in testCases)
            {
                RunTest message = new RunTest
                {
                    FullName = n.Attributes["fullname"].Value
                };

                helperBlue.Send<RunTest>(message);
            }

            //helperBlue.TeardownTransport();
            //helperGreen.TeardownTransport();

            helperBlue.Dispose();

            Console.WriteLine(" Press [enter] to exit.");
        }




    }
}

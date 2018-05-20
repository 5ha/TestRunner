using MessageModels;
using NUnit.Engine;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RabbitSender
{
    class Program
    {
        static void Main(string[] args)
        {

            TransportService.Helper helperBlue = new TransportService.Helper("Blue");
            TransportService.Helper helperGreen = new TransportService.Helper("Green");

            ITestEngine testEngine = TestEngineActivator.CreateInstance();

            var filterService = testEngine.Services.GetService<ITestFilterService>();
            var filterBuilder = filterService.GetTestFilterBuilder();
            filterBuilder.AddTest("SystemUnderTest.TestClass.PassingTest");

            TestPackage package = new TestPackage(@"C:\\Users\\shawn\\source\\repos\\TestNUnitRunner\\SystemUnderTest\\bin\\Debug\\SystemUnderTest.dll");

            ITestRunner runner = testEngine.GetRunner(package);

            var testSuites = runner.Explore(TestFilter.Empty);

            var testCases = testSuites.SelectNodes("//test-case");

            foreach (XmlNode n in testCases)
            {
                RunTest message = new RunTest
                {
                    FullName = n.Attributes["fullname"].Value
                };

                bus.Publish(message);
            }

            //helperBlue.TeardownTransport();
            //helperGreen.TeardownTransport();

            helperBlue.Dispose();
            helperGreen.Dispose();

            Console.WriteLine(" Press [enter] to exit.");
        }




    }
}

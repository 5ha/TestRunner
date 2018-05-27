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
            string build = args[0];

            TransportService.Helper helperBuild = new TransportService.Helper("build_queue");

            List<string> commands = new List<string>
            {
                "TestRunner", build, "test_responses", "c:\\app"
            };
                
            helperBuild.Send(new RunBuild {Build = build, Commands = commands, ContainerImage =  "testrunner"});

            TransportService.Helper helperBlue = new TransportService.Helper(build);

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
                    Build = build,
                    FullName = n.Attributes["fullname"].Value
                };

                for (int i = 0; i < 10; i++)
                {
                    helperBlue.Send<RunTest>(message);
                }
            }

            //helperBlue.TeardownTransport();
            //helperGreen.TeardownTransport();

            helperBlue.Dispose();
            helperBuild.Dispose();

            Console.WriteLine(" Press [enter] to exit.");
        }




    }
}

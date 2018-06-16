using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RabbitSender
{
    class Program
    {
        static ISender BuildInstructionSender;
        static ISender TestInstructionSender;
        static string directoryToSearch = @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish\SystemUnderTest";
        static ITestEngine testEngine;

        static void Main(string[] args)
        {
            testEngine = TestEngineActivator.CreateInstance();

            Console.WriteLine("Configuring Build Instruction Sender");
            BuildInstructionSender = ConfigureSender("build_queue");

            string build;

            do
            {
                Console.WriteLine("Type in a build number and press enter to run the build");
                Console.WriteLine("Press enter on it's own to quit");
                build = Console.ReadLine();
                if (!string.IsNullOrEmpty(build))
                {
                    Console.WriteLine("Please wait ...");
                    KickOffBuild(build);
                    Console.WriteLine("OK");
                    Console.WriteLine("===================================================");
                }
            } while (!string.IsNullOrEmpty(build));


            Console.WriteLine("Disposing Build Instruction Sender");
            BuildInstructionSender.Dispose();
            TestInstructionSender.Dispose();
            Console.WriteLine("All done");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void KickOffBuild(string build)
        {
            Console.WriteLine("Configuring Test Instruction Sender");
            TestInstructionSender = ConfigureSender($"{build}_request");

            Console.WriteLine("Sending Test Instructions");
            SendTestInstructions(TestInstructionSender, build, directoryToSearch);

            Console.WriteLine("Sending Build Instruction");
            BuildInstructionSender.Send(CreateBuildInstruction(build));
        }

        private static ISender ConfigureSender(string queue)
        {
            IQueueBuilder queueBuilder = new RabbitBuilder();
            ISender sender = queueBuilder.ConfigureTransport("my-rabbit", "remote", "remote")
                .ISendTo(queue)
                .Build();
            return sender;
        }

        private static RunBuild CreateBuildInstruction(string build)
        {
            List<string> commands = new List<string>
            {
                "TestRunner", "my-rabbit", "remote", "remote", $"{build}_request", $"{build}_response", "c:\\app"
            };

            return new RunBuild { Build = build, Commands = commands, ContainerImage = "testrunner" };
        }

        private static void SendTestInstructions(ISender sender, string build, string directoryToSearch)
        {


            string baseDirectory = @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish\SystemUnderTest";

            var files = Directory.GetFiles(baseDirectory, "*.dll", SearchOption.AllDirectories);

            TestPackage package = new TestPackage(files);

            using (ITestRunner runner = testEngine.GetRunner(package))
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

                    for (int i = 0; i < 10; i++)
                    {
                        TestInstructionSender.Send<RunTest>(message);
                    }
                }
            }
        }
    }
}

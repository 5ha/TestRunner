using HiQ.Builders;
using HiQ.Interfaces;
using InContainerShared;
using log4net.Config;
using MessageModels;
using NUnit.Engine;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;

namespace TestRunner
{
    class Program 
    {
        static ISender sender;
        static IReceiver receiver;

        static void Main(string[] args)
        {
            Console.WriteLine("Running tester in container");

            string requestQueueName = System.Environment.GetEnvironmentVariable("TESTER_REQUEST_QUEUE");

            Console.WriteLine($"requestQueueName: {requestQueueName}");


            string responseQueueName = System.Environment.GetEnvironmentVariable("TESTER_RESPONSE_QUEUE");

            Console.WriteLine($"responseQueueName: {responseQueueName}");

            string instanceName = System.Environment.GetEnvironmentVariable("TESTER_INSTANCE");

            Console.WriteLine($"instanceName: {instanceName}");

            string queueServer = System.Environment.GetEnvironmentVariable("TESTER_SERVER");

            Console.WriteLine($"queueServer: {queueServer}");

            string queueVhost = System.Environment.GetEnvironmentVariable("TESTER_VHOST");

            Console.WriteLine($"queueVhost: {queueVhost}");

            string queueUsername = System.Environment.GetEnvironmentVariable("TESTER_USERNAME");

            Console.WriteLine($"queueUsername: {queueUsername}");

            string queuePassword = System.Environment.GetEnvironmentVariable("TESTER_PASSWORD");

            Console.WriteLine($"queuePassword: {queuePassword}");


            string directoryToSearch = ConfigurationManager.AppSettings["directoryToSearch"];

            Console.WriteLine($"directoryToSearch: {directoryToSearch}");

            string listTests = System.Environment.GetEnvironmentVariable("TESTER_LISTTESTS");

            Console.WriteLine($"listTests: {listTests}");

            if (!string.IsNullOrEmpty(listTests))
            {
                ListTests(directoryToSearch);

                return;
            }

            log4net.GlobalContext.Properties["LogName"] = $"{instanceName}.log";

            XmlConfigurator.Configure();

            IQueueBuilder queueBuilder = new RabbitBuilder();
            receiver =
                queueBuilder.ConfigureTransport(queueServer, queueVhost, queueUsername, queuePassword)
                .IReceiveFrom(requestQueueName)
                .IReceiveUntilNoMoreMessages(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(20), ShutDown)
                .Build();

            queueBuilder = new RabbitBuilder();
            sender = queueBuilder.ConfigureTransport(queueServer, queueVhost, queueUsername, queuePassword)
                .ISendTo(responseQueueName)
                .Build();

            var files = Directory.GetFiles(directoryToSearch, "*.dll", SearchOption.AllDirectories).ToList();

            TestExecutor executor = new TestExecutor(files);

            receiver.Receive<RunTest>((m) =>
            {
                try
                {
                    Console.WriteLine($"Running {m.FullName} ...");
                    var responseXML = executor.Execute(m);

                    var responseNode = responseXML.SelectSingleNode("//test-case");
                    var testResult = responseNode.Attributes["result"].Value;
                    Console.WriteLine($"{m.FullName} : {testResult.ToUpper()}");

                    sender.Send(new TestResult { TestRequestId = m.TestRequestId, Build = m.Build, FullName = m.FullName });
                } catch(Exception e)
                {
                    sender.Send(new StatusMessage { Application = "TestRunner", Process = instanceName,  Error = e.Message });
                }
            });

            Console.WriteLine("Listening ...");
        }

        private static void ListTests(string directoryToSearch)
        {
            using (ITestEngine testEngine = TestEngineActivator.CreateInstance())
            {
                Console.WriteLine("In test engine instance");

                var files = Directory.GetFiles(directoryToSearch, "*.dll", SearchOption.AllDirectories);

                Console.WriteLine($"Found {files.Count()} to search");

                TestPackage package = new TestPackage(files);

                Console.WriteLine("Test package created");


                using (ITestRunner runner = testEngine.GetRunner(package))
                {
                    Console.WriteLine("In runner");

                    var testSuites = runner.Explore(TestFilter.Empty);

                    var testCases = testSuites.SelectNodes("//test-case");

                    foreach (XmlNode n in testCases)
                    {
                        Console.WriteLine(n.Attributes["fullname"].Value);
                    }
                }
            }
        }
        private static void ShutDown()
        {
            receiver.Dispose();
            sender.Dispose();
            Console.WriteLine("Environment exiting.");
            Environment.Exit(0);
        }
    }
}

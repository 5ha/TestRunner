using HiQ.Builders;
using HiQ.Interfaces;
using InContainerShared;
using log4net.Config;
using MessageModels;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace TestRunner
{
    class Program 
    {
        static ISender sender;
        static IReceiver receiver;

        static void Main(string[] args)
        {

            string requestQueueName = System.Environment.GetEnvironmentVariable("TESTER_REQUEST_QUEUE");
            string responseQueueName = System.Environment.GetEnvironmentVariable("TESTER_RESPONSE_QUEUE");
            string instanceName = System.Environment.GetEnvironmentVariable("TESTER_INSTANCE");
            string queueServer = System.Environment.GetEnvironmentVariable("TESTER_SERVER");
            string queueVhost = System.Environment.GetEnvironmentVariable("TESTER_VHOST");
            string queueUsername = System.Environment.GetEnvironmentVariable("TESTER_USERNAME");
            string queuePassword = System.Environment.GetEnvironmentVariable("TESTER_PASSWORD");
            string directoryToSearch = System.Environment.GetEnvironmentVariable("TESTER_SEARCHDIR");

            log4net.GlobalContext.Properties["LogName"] = $"{instanceName}.log";

            XmlConfigurator.Configure();

            IQueueBuilder queueBuilder = new RabbitBuilder();
            receiver =
                queueBuilder.ConfigureTransport(queueServer, queueVhost, queueUsername, queuePassword)
                .IReceiveFrom(requestQueueName)
                .IReceiveUntilNoMoreMessages(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(5), ShutDown)
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

                    sender.Send(new TestResult { Build = m.Build, FullName = m.FullName, Result = responseXML });
                } catch(Exception e)
                {
                    sender.Send(new StatusMessage { Application = "TestRunner", Process = instanceName,  Error = e.Message });
                }
            });

            Console.WriteLine("Listening ...");
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

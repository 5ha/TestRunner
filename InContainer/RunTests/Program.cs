using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageModels;
using NUnit.Engine;
using System.Xml;
using System.IO;
using HiQ.Interfaces;
using HiQ.Builders;
using System.Configuration;
using InContainerShared;

namespace TestRunner
{

    class Program 
    {
        static ISender sender;
        static IReceiver receiver;

        static void Main(string[] args)
        {
            string instanceName = Environment.GetEnvironmentVariable("instance");
            string requestQueueName = args[0];
            string responseQueueName = args[1];

            string queueServer = ConfigurationManager.AppSettings["queueServer"];
            string queueUsername = ConfigurationManager.AppSettings["queueUsername"];
            string queuePassword = ConfigurationManager.AppSettings["queuePassword"];
            string directoryToSearch = ConfigurationManager.AppSettings["directoryToSearch"];

            IQueueBuilder queueBuilder = new RabbitBuilder();
            receiver =
                queueBuilder.ConfigureTransport(queueServer, queueUsername, queuePassword)
                .IReceiveFrom(requestQueueName)
                .IReceiveUntilNoMoreMessages(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(5), ShutDown)
                .Build();

            queueBuilder = new RabbitBuilder();
            sender = queueBuilder.ConfigureTransport(queueServer, queueUsername, queuePassword)
                .ISendTo(responseQueueName)
                .Build();

            var files = Directory.GetFiles(directoryToSearch, "*.dll", SearchOption.AllDirectories).ToList();

            TestExecutor executor = new TestExecutor(files);

            receiver.Receive<RunTest>((m) =>
            {
                Console.WriteLine($"Running {m.FullName} ...");
                var responseXML = executor.Execute(m);

                var responseNode = responseXML.SelectSingleNode("//test-case");
                var testResult = responseNode.Attributes["result"].Value;
                Console.WriteLine($"{m.FullName} : {testResult.ToUpper()}");

                sender.Send(new TestResult { Build = m.Build, FullName = m.FullName, Result = responseXML });
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

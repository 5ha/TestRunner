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

namespace TestRunner
{
    class Program 
    {
        static ISender sender;
        static IReceiver receiver;

        static void Main(string[] args)
        {
            string instanceName = Environment.GetEnvironmentVariable("instance");
            string queueServer = args[0];
            string queueUsername = args[1];
            string queuePassword = args[2];
            string requestQueueName = args[3];
            string responseQueueName = args[4];
            string directoryToSearch = args[5];

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

    public class Listener : ITestEventListener
    {
        public void OnTestEvent(string report)
        {
            var x = report;
        }
    }
}

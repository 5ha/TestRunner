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
            string instanceName = args[0];
            string requestQueueName = args[1];
            string responseQueueName = args[2];

            log4net.GlobalContext.Properties["LogName"] = $"{instanceName}.log";

            XmlConfigurator.Configure();

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

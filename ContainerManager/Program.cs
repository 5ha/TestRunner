using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerManager
{
    class Program
    {
        private static List<Process> processes = new List<Process>();

        static string _queueServer;
        static string _queueVhost;
        static string _queueUsername;
        static string _queuePassword;

        static void Main(string[] args)
        {
            _queueServer = ConfigurationManager.AppSettings["queueServer"];
            _queueVhost = ConfigurationManager.AppSettings["queueVhost"];
            _queueUsername = ConfigurationManager.AppSettings["queueUsername"];
            _queuePassword = ConfigurationManager.AppSettings["queuePassword"];

            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            int numberOfProcesses = Int32.Parse(args[0]);

            // Initialise processes
            CancellationTokenSource cancellation = new CancellationTokenSource();
            for (int i = 0; i < numberOfProcesses; i++)
            {
                var proc = new Process($"process{i}", cancellation.Token);
                processes.Add(proc);
                proc.Initialise();
            }

            // Subscribe to Build Messages
            IQueueBuilder queueBuilder = new RabbitBuilder();
            IReceiver receiver = queueBuilder.ConfigureTransport(_queueServer, _queueVhost, _queueUsername, _queuePassword)
                .IReceiveFrom(QueueNames.Build())
                .IReceiveForever()
                .Build();

            receiver.Receive<RunBuild>((m) => {
                RunBuild(m);
            });

            Console.ReadLine();
            Console.WriteLine("Cancelling processes ...");
            cancellation.Cancel();
            Console.WriteLine("Processes cancelled.");
        }



        public static void RunBuild(RunBuild build)
        {
            IQueueBuilder queueBuilder = new RabbitBuilder();
            List<Task> tasks = new List<Task>();

            using (ISender statusMessageSender = queueBuilder.ConfigureTransport(_queueServer, _queueVhost, _queueUsername, _queuePassword)
                .ISendTo(QueueNames.Status(build.Build))
                .Build())
            {
                SendStatusMessage(statusMessageSender, $"Starting build {build.Build}");
            }

            foreach (var proc in processes)
            {
                tasks.Add(proc.RunBuild(build));
            }

            // Wait for at least one task to complete this build before moving onto the next build
            Task.WaitAny(tasks.ToArray());

            // Don't send anymore messages to the status queue after this point as it might have been removed already
            // and we don't want to re-create it
        }

        private static void SendStatusMessage(ISender sender, string message)
        {
            sender.Send(StatusReport(message));
            Console.WriteLine(message);
        }

        private static StatusMessage PrepareStatusMessage()
        {
            StatusMessage message = new StatusMessage
            {
                Machine = Environment.MachineName,
                Application = "ContainerManager"
            };
            return message;
        }

        private static StatusMessage StatusReport(string message)
        {
            StatusMessage m = PrepareStatusMessage();
            m.Message = message;
            return m;
        }

    }
}

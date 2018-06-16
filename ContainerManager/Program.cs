using HiQ.Builders;
using HiQ.Interfaces;
using MessageModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerManager
{
    class Program
    {
        private static List<Process> processes = new List<Process>();

        static void Main(string[] args)
        {
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
            IReceiver receiver = queueBuilder.ConfigureTransport("my-rabbit", "remote", "remote")
                .IReceiveFrom("build_queue")
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
            List<Task> tasks = new List<Task>();

            Console.WriteLine($"Running {build.Build}");

            foreach(var proc in processes)
            {
                tasks.Add(proc.RunBuild(build));
            }

            Console.WriteLine("Waiting for tasks");

            Task.WaitAny(tasks.ToArray());

            Console.WriteLine("A task has completed");

            foreach (var proc in processes)
            {
                proc.RunBuild(build);
            }

            Console.WriteLine("All completed ...removing build");
        }

    }
}

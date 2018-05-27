using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using MessageModels;

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
            TransportService.Helper helperBuild = new TransportService.Helper("build_queue");
            CancellationTokenSource cancellation = new CancellationTokenSource();

            for(int i = 0; i < numberOfProcesses; i++)
            {
                var proc = new Process($"process{i}", cancellation.Token);
                processes.Add(proc);
                proc.Initialise();
            }

            RunBuild(new RunBuild {
                ContainerImage = "testrunner",
                Build = "Build1",
                Commands = new List<string>
                {
                    "TestRunner","Build1","test_responses","c:\\app"
                }
            });

            Console.ReadLine();
            cancellation.Cancel();
        }

        public static void RunBuild(RunBuild build)
        {
            List<Task> tasks = new List<Task>();

            Console.Write("Addin Builds");

            foreach(var proc in processes)
            {
                tasks.Add(proc.RunBuild(build));
            }

            Console.Write("Waiting for tasks");

            Task.WaitAny(tasks.ToArray());

            Console.Write("A task has completed");

            foreach (var proc in processes)
            {
                proc.RunBuild(build);
            }

            Console.Write("Removing build");
        }

    }
}

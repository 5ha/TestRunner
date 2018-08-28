using InContainerShared;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace RunTestsLocally
{
    class Program
    {
        static void Main(string[] args)
        {
            StringListener listener = new StringListener();

            var files = Directory.GetFiles(ConfigurationManager.AppSettings["directoryToSearch"], "*.dll", SearchOption.AllDirectories).ToList();
            TestExecutor executor = new TestExecutor(files);
            executor.ExecuteAll(listener);

            Console.WriteLine(listener.TestResults);
        }
    }
}

using InContainerShared;
using System.Configuration;
using System.IO;
using System.Linq;

namespace RunTestsLocally
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(ConfigurationManager.AppSettings["directoryToSearch"], "*.dll", SearchOption.AllDirectories).ToList();
            TestExecutor executor = new TestExecutor(files);
            executor.ExecuteAll();
        }
    }
}

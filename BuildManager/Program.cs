using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string build;

            try
            {
                do
                {
                    Console.WriteLine("Type in a build number and press enter to run the build");
                    Console.WriteLine("Press enter on it's own to quit");
                    build = Console.ReadLine();
                    if (!string.IsNullOrEmpty(build))
                    {
                        Console.WriteLine("Please wait ...");
                        await KickOffBuild(build);
                        Console.WriteLine("OK");
                        Console.WriteLine("===================================================");
                    }
                } while (!string.IsNullOrEmpty(build));

            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static async Task KickOffBuild(string build)
        {
            using (var processor = new BuildProcessor("my-rabbit", "remote", "remote", @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish", @"C:\Users\shawn\source\repos\TestNUnitRunner\Publish\SystemUnderTest", "shawnseabrook", "myimage", build))
            {
                await processor.StartBuild(build);
            }
        }
    }
}

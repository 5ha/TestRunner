using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ContainerManager
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {

            ContainerHelper helper = new ContainerHelper();
            string containerName = "testcontainer";

            if (helper.ContainerExists(containerName))
            {
                await helper.RemoveContainer(containerName);
            }

            var res = helper.CreateContainer(containerName);

            await helper.StartContainer(res.ID);

            Console.WriteLine("Container started");
            Console.ReadLine();

        }

    }
}

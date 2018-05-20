using MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitReceiverBlueB
{
    class Program
    {
        static void Main(string[] args)
        {
            TransportService.Helper helper = new TransportService.Helper("Blue");

            helper.Subscribe<RunTest>((m) =>
            {
                Console.WriteLine($"Received {m.FullName}");
            }, () => {
                Console.WriteLine("Environment exiting.");
                helper.Dispose();
                Environment.Exit(0);
                });

            Console.WriteLine("Listening ...");
        }
    }
}

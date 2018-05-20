using MessageModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitReceiverBlue
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

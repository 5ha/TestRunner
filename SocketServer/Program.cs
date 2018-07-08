using log4net.Config;
using Model;
using ReactiveSockets;
using SocketProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var port = 1055;
            if (args.Length > 0)
                port = int.Parse(args[0]);

            var server = new ReactiveListener(port);

            server.Connections.Subscribe(socket =>
            {
                Console.WriteLine("New socket connected {0}", socket.GetHashCode());

                var protocol = new StringChannel(socket);

  
                protocol.Receiver.Subscribe(
                    s => {
                        Console.WriteLine(s);

                        BuildRunRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildRunRequest>(s);

                        BuildRunner buildRunner = new BuildRunner(protocol, request);
                        buildRunner.StartBuild().Wait();
                    }
                    
                    ,
                    e => Console.WriteLine(e),
                    () => Console.WriteLine("Socket receiver completed"));

                

                socket.Disconnected += (sender, e) => Console.WriteLine("Socket disconnected {0}", sender.GetHashCode());
                socket.Disposed += (sender, e) => Console.WriteLine("Socket disposed {0}", sender.GetHashCode());
            });

            server.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
    }
}

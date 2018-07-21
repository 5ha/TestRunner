using BuildManager.Model;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using log4net.Config;
using MessageModels;
using Model;
using Newtonsoft.Json;
using ReactiveSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        private static ReactiveClient client;
        private static ILog _log;
        private static bool _isShuttingDown;

        private static int _port = 1055;
        private static string _host = "127.0.0.1";

        static async Task RunClientAsync(BuildRunRequest request)
        {
            var group = new MultithreadEventLoopGroup();

            string targetHost = null;

            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new SocketClientHandler(request));
                    }));

                IChannel clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_host), _port));

                Console.ReadLine();

                await clientChannel.CloseAsync();
            }
            finally
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }

        static void Main(string[] args)
        {
                XmlConfigurator.Configure();

                if (args.Length == 0)
                    throw new ArgumentException("Usage: reactiveclient host build");

                var host = args[0];

                var port = 1055;

                var build = args[1];
                var image = args[2];

                BuildRunRequest request = new BuildRunRequest
                {
                    Build = build,
                    Image = image
                };

                RunClientAsync(request).Wait();
        }



        /**
         * echo ##teamcity[message text='compiler output']
echo ##teamcity[message text='compiler error' status='ERROR']

echo ##teamcity[testStarted name='MyTest.test2']
echo ##teamcity[testFailed type='comparisonFailure' name='MyTest.test2' message='failure message' details='message and stack trace' expected='expected value' actual='actual value']
echo ##teamcity[testFinished name='MyTest.test2']

echo ##teamcity[testStarted name='MyTest.test3']
echo ##teamcity[testFinished name='MyTest.test3']
         * **/
    }

    //C:\Users\shawn\source\repos\TestNUnitRunner\SocketClient\bin\Debug\SocketClient.exe 127.0.0.1 37


}

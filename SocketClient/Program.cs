using BuildManager.Model;
using log4net;
using log4net.Config;
using MessageModels;
using Model;
using Newtonsoft.Json;
using ReactiveSockets;
using SocketProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        static void Main(string[] args)
        {
            try
            {
                XmlConfigurator.Configure();

                _log = LogManager.GetLogger("SocketClient");

                string processInfo = $"Socket CLient running PID: {Process.GetCurrentProcess().Id}";
                OutputMessage(processInfo);

                if (args.Length == 0)
                    throw new ArgumentException("Usage: reactiveclient host build");

                var host = args[0];

                var port = 1055;

                var build = args[1];
                var image = args[2];

                client = new ReactiveClient(host, port);
                var protocol = new StringChannel(client);

                protocol.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(
                    s =>
                    {
                        try
                        {
                            var obj = JsonConvert.DeserializeObject(s, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

                            StatusMessage statusMessage = obj as StatusMessage;
                            TestExecutionResult testResult = obj as TestExecutionResult;

                            if (testResult != null)
                            {
                                OutputTestMessage(testResult);
                            }

                            if (statusMessage != null)
                            {
                                if (statusMessage.Message == "DONE")
                                {
                                    ShutDown(0);
                                }
                                OutputStatusMessage(statusMessage);
                            }
                            else
                            {
                                OutputMessage(obj.GetType().FullName);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            ShutDown(1);
                        }
                    },
                    e => { OutputException(e); ShutDown(1); },
                    () => OutputMessage("Socket receiver completed"));

                client.ConnectAsync().Wait();

                BuildRunRequest request = new BuildRunRequest
                {
                    Build = build,
                    Image = image
                };

                string mess = JsonConvert.SerializeObject(request);

                protocol.SendAsync(mess);
                OutputMessage($"Request build {build}");

                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
                //string line;
                //while ((line = Console.ReadLine()) != "") { }

                //while (client.IsConnected)
                //{
                //    Task.Delay(1000).Wait();
                //}

                //string line = null;

                //while ((line = Console.ReadLine()) != "")
                //{
                //    if (line == "r")
                //    {
                //        OutputStatusMessage("Reconnecting...");
                //        client.Disconnect();
                //        client.ConnectAsync().Wait();
                //        Console.WriteLine("IsConnected = {0}", client.IsConnected);
                //    }
                //    else
                //    {
                //        OutputStatusMessage("Sending");

                //    }
                //}
            }
            catch (Exception e)
            {
                OutputException(e);
                ShutDown(1);
            }
        }

        private static void ShutDown(int code)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            
            Environment.Exit(code);
        }

        private static string Escape(string s)
        {
            if (s == null) return null;

            s = s.Replace("|", "||");
            s = s.Replace("'", "|'");
            s = s.Replace("\n", "|n");
            s = s.Replace("\r", "|r");
            s = s.Replace("[", "|[");
            s = s.Replace("]", "|]");

            return s;
        }

        private static void OutputMessage(string message)
        {
            string mess = $"##teamcity[message text='{Escape(message)}']";
            _log.Info(mess);
            Console.WriteLine(mess);
        }

        private static void OutputException(Exception e)
        {
            string mess = $"##teamcity[message text='{Escape(e.Message)}'  status='ERROR']";
            Console.WriteLine(mess);
            _log.Error(mess);
            //Console.WriteLine($"##teamcity[message text='{Escape(e.StackTrace)}'  status='ERROR']");
        }

        private static void OutputStatusMessage(StatusMessage mess)
        {
            string m;

            if (!string.IsNullOrEmpty(mess.Message))
            {
                m = $"##teamcity[message text='{Escape(mess.Message)}']";
                Console.WriteLine(m);
                _log.Info(m);
            }
            if (!string.IsNullOrEmpty(mess.Warning))
            {
                m = $"##teamcity[message text='{Escape(mess.Warning)}'  status='WARNING']";
                Console.WriteLine(m);
                _log.Warn(m);
            }
            if (!string.IsNullOrEmpty(mess.Error))
            {
                m = $"##teamcity[message text='{Escape(mess.Error)}'  status='ERROR']";
                _log.Error(m);
                Console.WriteLine(m);
                ShutDown(1);
            }
            
        }

        private static void OutputTestMessage(TestExecutionResult result)
        {
            string m;
            m = $"##teamcity[testStarted name='{Escape(result.FullName)}']";
            Console.WriteLine(m);

            m = $"##teamcity[testFinished name='{Escape(result.FullName)}']";
            Console.WriteLine(m);
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

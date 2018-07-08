using BuildManager.Model;
using MessageModels;
using Model;
using Newtonsoft.Json;
using ReactiveSockets;
using SocketProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    throw new ArgumentException("Usage: reactiveclient host build");

                var host = args[0];

                var port = 1055;

                var build = args[1];
                var image = args[2];

                //if (args.Length > 1)
                //    port = int.Parse(args[1]);

                var client = new ReactiveClient(host, port);
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
                                    client.Disconnect();
                                    Environment.Exit(0);
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
                        }
                    },
                    e => OutputException(e),
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
                Console.WriteLine("Failed: {0}", e);
            }
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
            Console.WriteLine($"##teamcity[message text='{Escape(message)}']");
        }

        private static void OutputException(Exception e)
        {
            Console.WriteLine($"##teamcity[message text='{Escape(e.Message)}'  status='ERROR']");
            Console.WriteLine($"##teamcity[message text='{Escape(e.StackTrace)}'  status='ERROR']");
        }

        private static void OutputStatusMessage(StatusMessage mess)
        {
            if (!string.IsNullOrEmpty(mess.Message))
            {
                Console.WriteLine($"##teamcity[message text='{Escape(mess.Message)}']");
            }
            if (!string.IsNullOrEmpty(mess.Warning))
            {
                Console.WriteLine($"##teamcity[message text='{Escape(mess.Warning)}'  status='WARNING']");
            }
            if (!string.IsNullOrEmpty(mess.Error))
            {
                Console.WriteLine($"##teamcity[message text='{Escape(mess.Error)}'  status='ERROR']");
            }
            
        }

        private static void OutputTestMessage(TestExecutionResult result)
        {
            Console.WriteLine($"##teamcity[testStarted name='{Escape(result.FullName)}']");
            Console.WriteLine($"##teamcity[testFinished name='{Escape(result.FullName)}']");
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

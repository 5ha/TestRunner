using BuildManager.Model;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using log4net;
using MessageModels;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    public class SocketClientHandler : ChannelHandlerAdapter
    {
        readonly IByteBuffer byteBuffer;
        readonly ILog _log;
        private static bool _isShuttingDown;

        public SocketClientHandler(BuildRunRequest request)
        {
            _log = LogManager.GetLogger("SocketClient");
            string processInfo = $"Socket CLient running PID: {Process.GetCurrentProcess().Id}";
            OutputMessage(processInfo);

            string mess = JsonConvert.SerializeObject(request);
            this.byteBuffer = Unpooled.Buffer(256);
            byte[] byteArray = Encoding.UTF8.GetBytes(mess);
            this.byteBuffer.WriteBytes(byteArray);
        }

        public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.byteBuffer);

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                try
                {
                    string s = byteBuffer.ToString(Encoding.UTF8);

                    _log.Info($"RECEIVED RAW: {s}");
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
                            OutputMessage("Test run complete");
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
                    HanldeException(ex);
                    ShutDown(1);
                }
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }

        private void ShutDown(int code)
        {
            _isShuttingDown = true;
            //if (client.IsConnected)
            //{
            //    client.Disconnect();
            //}

            Environment.Exit(code);
        }

        private string Escape(string s)
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

        private void OutputMessage(string message)
        {
            string mess = $"##teamcity[message text='{Escape(message)}']";
            _log.Info(mess);
            Console.WriteLine(mess);
        }

        private void HanldeException(Exception ex)
        {
            Exception currentExcepition = ex;

            while (currentExcepition != null)
            {
                OutputException(currentExcepition);
                currentExcepition = currentExcepition.InnerException;

            }

        }

        private void OutputException(Exception e)
        {
            string mess = $"##teamcity[message text='{Escape(e.Message)}'  status='ERROR']";
            Console.WriteLine(mess);
            _log.Error(mess);

            // do not shutdown at this point, we might still be handling inner excepstins
            //Console.WriteLine($"##teamcity[message text='{Escape(e.StackTrace)}'  status='ERROR']");
        }

        private void OutputStatusMessage(StatusMessage mess)
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

        private void OutputTestMessage(TestExecutionResult result)
        {
            string m;
            m = $"##teamcity[testStarted name='{Escape(result.FullName)}']";
            Console.WriteLine(m);

            m = $"##teamcity[testFinished name='{Escape(result.FullName)}']";
            Console.WriteLine(m);
        }
    }
}

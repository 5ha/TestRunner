using DockerUtils;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    public class SocketServerHandler :  ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                Action<string> sender = (s) =>
                {
                    IByteBuffer byteBuffer = Unpooled.Buffer(256);
                    byte[] byteArray = Encoding.UTF8.GetBytes(s);
                    byteBuffer.WriteBytes(byteArray);
                    context.WriteAndFlushAsync(byteBuffer);
                };

                try
                {
                    //ContainerHelper helper = new ContainerHelper();
                    //var res = helper.ListContainers(true).Result;

                    var res = Task.Run<int>(() => { Task.Delay(10000).Wait(); return 1; }).Result;

                    //WebClient client = new WebClient();

                        //// Add a user agent header in case the 
                        //// requested URI contains a query.

                        //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                        //Stream data = client.OpenRead("http://www.google.com");
                        //StreamReader reader = new StreamReader(data);
                        //string s = reader.ReadToEnd();

                        string m = buffer.ToString(Encoding.UTF8);
                    Console.WriteLine(m);

                    BuildRunRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildRunRequest>(m);

                    BuildRunner buildRunner = new BuildRunner(request, sender);
                    buildRunner.StartBuild().Wait();
                }catch(Exception e)
                {
                   var x = e.Message;
                }
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}

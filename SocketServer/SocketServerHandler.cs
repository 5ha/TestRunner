using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
                string m = buffer.ToString(Encoding.UTF8);
                Console.WriteLine(m);

                BuildRunRequest request = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildRunRequest>(m);

                BuildRunner buildRunner = new BuildRunner(request, sender);
                buildRunner.StartBuild().Wait();
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

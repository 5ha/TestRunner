using HiQ.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace HiQ.Implementations
{
    public class RabbitSender : BaseRabbitEndpoint, ISender
    {
        public RabbitSender(string hostName, string vHost, string userName, string password, string exchangeName, string pathName)
            :base(hostName, vHost, userName, password, exchangeName, pathName)
        {

        }

        public void Send<TMessageType>(TMessageType message)
        {
            string serialisedMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialisedMessage);
            Channel.BasicPublish(exchange: _exchangeName,
                                 routingKey: _pathName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}

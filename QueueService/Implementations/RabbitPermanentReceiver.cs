using QueueService.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Implementations
{
    public class RabbitPermanentReceiver : BaseRabbitEndpoint, IReceiver
    {
        public RabbitPermanentReceiver(string hostName, string vHost, string userName, string password, string exchangeName, string pathName)
            : base(hostName, vHost, userName, password, exchangeName, pathName)
        {

        }

        public void Receive<TMessageType>(Action<TMessageType> onReceived)
        {
            var consumer = new EventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                TMessageType typedMessage = JsonConvert.DeserializeObject<TMessageType>(message);
                onReceived(typedMessage);

                Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            Channel.BasicConsume(queue: _pathName,
                                 autoAck: false,
                                 consumer: consumer);
        }
    }
}

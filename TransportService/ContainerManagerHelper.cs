using MessageModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportService
{
    public class ContainerManagerHelper
    {
        private string _requestExchange;
        private string _requestPath;

        public ContainerManagerHelper(string requestExchange, string requestPath)
        {
            _requestExchange = requestExchange;
            _requestPath = requestPath;
        }

        private ConnectorBase __requestConnector;
        private ConnectorBase _requestConnector
        {
            get
            {
                if (__requestConnector == null)
                {
                    __requestConnector = new ConnectorBase(_requestExchange, _requestPath);
                }
                return __requestConnector;
            }
        }

        public void Subscribe(Func<RunBuild, Task> onReceived)
        {
            var consumer = new EventingBasicConsumer(_requestConnector.Channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                RunBuild typedMessage = JsonConvert.DeserializeObject<RunBuild>(message);
                onReceived(typedMessage).Wait();

                _requestConnector.Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _requestConnector.Channel.BasicConsume(queue: _requestPath,
                                 autoAck: false,
                                 consumer: consumer);
        }
    }
}

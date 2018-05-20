using MessageModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransportService
{
    public class TestClientHelper : IDisposable
    {
        private string _requestExchange;
        private string _requestPath;
        private string _responseExchange;
        private string _responsePath;
        private Action _shutdownAction;

        public TestClientHelper(string requestExchange, string requestPath, 
            string responseExchange, string responsePath, Action shutdownAction)
        {
            _requestExchange = requestExchange;
            _requestPath = requestPath;
            _responseExchange = responseExchange;
            _responsePath = responsePath;
            _shutdownAction = shutdownAction;
        }

        private ConnectorBase __requestConnector;
        private ConnectorBase _requestConnector
        {
            get
            {
                if(__requestConnector == null)
                {
                    __requestConnector = new ConnectorBase(_requestExchange, _requestPath);
                }
                return __requestConnector;
            }
        }

        private ConnectorBase __responseConnector;
        private ConnectorBase _responseConnector
        {
            get
            {
                if (__responseConnector == null)
                {
                    __responseConnector = new ConnectorBase(_responseExchange, _responsePath);
                }
                return __responseConnector;
            }
        }

        public void Subscribe<TMessageType>(Action<TMessageType> onReceived)
        {
            CancellationTokenSource cancelShutdown = new CancellationTokenSource();

            // As we are starting up give extra time to get messages
            ShutDown(TimeSpan.FromSeconds(30), cancelShutdown).ConfigureAwait(false);

            var consumer = new EventingBasicConsumer(_requestConnector.Channel);

            consumer.Received += (model, ea) =>
            {
                // There is a new message so we are not ready to shutdown yet
                cancelShutdown.Cancel();

                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                TMessageType typedMessage = JsonConvert.DeserializeObject<TMessageType>(message);
                onReceived(typedMessage);

                _requestConnector.Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                // Shutdown (unless we received a new message soon)
                cancelShutdown = new CancellationTokenSource();
                ShutDown(TimeSpan.FromSeconds(5), cancelShutdown).ConfigureAwait(false);
            };

            _requestConnector.Channel.BasicConsume(queue: _requestPath,
                                 autoAck: false,
                                 consumer: consumer);
        }

        private async Task ShutDown(TimeSpan delay, CancellationTokenSource cancellationSource)
        {
            await Task.Delay(delay); // Give the caller some time to cancel to cancel
            if (!cancellationSource.IsCancellationRequested)
            {
                Console.WriteLine("No more messages .. shutting down");
                _shutdownAction();
            }
        }

        public void SendTestResult(TestResult message)
        {
            _responseConnector.Send(message);
        }

        public void Dispose()
        {
            if (__requestConnector != null) __requestConnector.Dispose();
            __requestConnector = null;

            if (__responseConnector != null) __responseConnector.Dispose();
            __responseConnector = null;
        }
    }
}

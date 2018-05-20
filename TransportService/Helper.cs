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
    public class Helper : IDisposable
    {
        const string EXCHANGE_NAME = "test_requests";
        string _pathName;
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;
        private Action _shutdownAction;

        public Helper(string pathName)
        {
            _pathName = pathName;
        }

        public void EnsureTransport()
        {
            if (_channel != null) return;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: "direct");

            _channel.QueueDeclare(queue: _pathName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            _channel.QueueBind(queue: _pathName,
                                  exchange: "test_requests",
                                  routingKey: _pathName);

        }

        public void TeardownTransport()
        {
            _channel.QueueDelete(_pathName);
        }

        public void Send<TMessageType>(TMessageType message)
        {
            EnsureTransport();

            string serialisedMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serialisedMessage);
            _channel.BasicPublish(exchange: EXCHANGE_NAME,
                                 routingKey: _pathName,
                                 basicProperties: null,
                                 body: body);
        }

        public void Subscribe<TMessageType>(Action<TMessageType> onReceived, Action shutdownAction)
        {
            _shutdownAction = shutdownAction;

            EnsureTransport();
            CancellationTokenSource cancelShutdown = new CancellationTokenSource();
            ShutDown(cancelShutdown).ConfigureAwait(false);

            _consumer = new EventingBasicConsumer(_channel);

            _consumer.Received += (model, ea) =>
            {
                // There is a new message so we are not ready to shutdown yet
                cancelShutdown.Cancel();

                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                TMessageType typedMessage = JsonConvert.DeserializeObject<TMessageType>(message);
                onReceived(typedMessage);

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                // Shutdown (unless we received a new message soon)
                cancelShutdown = new CancellationTokenSource();
                ShutDown(cancelShutdown).ConfigureAwait(false);
            };
            _channel.BasicConsume(queue: _pathName,
                                 autoAck: false,
                                 consumer: _consumer);
        }

        private async Task ShutDown(CancellationTokenSource cancellationSource)
        {
            await Task.Delay(5000); // Give the caller 5 seconds to cancel
            if (!cancellationSource.IsCancellationRequested)
            {
                Console.WriteLine("No more messages .. shutting down");
                _channel.Close();
                _shutdownAction();
            }
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                if(_channel.IsOpen)
                _channel.Close();

                _channel = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }
    }
}

using Newtonsoft.Json;
using QueueService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueService.Implementations
{
    public class RabbitTemporaryReceiver : BaseRabbitEndpoint, IReceiver
    {
        private TimeSpan _startUpWaitTime;
        private TimeSpan _messageTimeout;
        private Action _onNoMoreMessages;

        public RabbitTemporaryReceiver(string hostName, string vHost, string userName, string password, string exchangeName, string pathName,
            TimeSpan startUpWaitTime, TimeSpan messageTimeout, Action onNoMoreMessages)
            : base(hostName, vHost, userName, password, exchangeName, pathName)
        {
            _startUpWaitTime = startUpWaitTime;
            _messageTimeout = messageTimeout;
            _onNoMoreMessages = onNoMoreMessages;
        }

        public void Receive<TMessageType>(Action<TMessageType> onReceived)
        {
            CancellationTokenSource cancelShutdown = new CancellationTokenSource();

            // As we are starting up give extra time to get messages
            CheckQueueTimedOut(_startUpWaitTime, cancelShutdown).ConfigureAwait(false);

            var consumer = new EventingBasicConsumer(Channel);

            consumer.Received += (model, ea) =>
            {
                // There is a new message so we are not ready to shutdown yet
                cancelShutdown.Cancel();

                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                TMessageType typedMessage = JsonConvert.DeserializeObject<TMessageType>(message);
                onReceived(typedMessage);

                Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                // Shutdown (unless we received a new message soon)
                cancelShutdown = new CancellationTokenSource();
                CheckQueueTimedOut(_messageTimeout, cancelShutdown).ConfigureAwait(false);
            };

            Channel.BasicConsume(queue: _pathName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        private async Task CheckQueueTimedOut(TimeSpan delay, CancellationTokenSource cancellationSource)
        {
            await Task.Delay(delay); // Give the caller some time to cancel to cancel
            if (!cancellationSource.IsCancellationRequested)
            {
                _onNoMoreMessages();
            }
        }
    }
}

using QueueService.Implementations;
using QueueService.Interfaces;
using System;

namespace QueueService.Builders
{
    public class RabbitBuilder : IQueueBuilder, IDirectionSelector, IReceiveTypeSelector, ISenderBuilder, ITemporaryReceiverBuilder, IPermanentReceiverBuilder
    {
        string _queueName;
        string _hostName;
        string _vHost;
        string _userName;
        string _password;

        private TimeSpan _startupTime;
        private TimeSpan _messageWaitTimeout;
        private Action _whenNoMoreMessages;

        IDirectionSelector IQueueBuilder.ConfigureTransport(string hostName, string vHost, string userName, string password)
        {
            _hostName = hostName;
            _vHost = vHost;
            _userName = userName;
            _password = password;

            return this;
        }

        ISenderBuilder IDirectionSelector.ISendTo(string queueName)
        {
            _queueName = queueName;

            return this;
        }

        IReceiveTypeSelector IDirectionSelector.IReceiveFrom(string queueName)
        {
            _queueName = queueName;

            return this;
        }

        IPermanentReceiverBuilder IReceiveTypeSelector.IReceiveForever()
        {
            return this;
        }

        ITemporaryReceiverBuilder IReceiveTypeSelector.IReceiveUntilNoMoreMessages(TimeSpan startupTime, TimeSpan messageWaitTimeout, Action whenNoMoreMessages)
        {
            _startupTime = startupTime;
            _messageWaitTimeout = messageWaitTimeout;
            _whenNoMoreMessages = whenNoMoreMessages;

            return this;
        }

        ISender ISenderBuilder.Build()
        {
            return new RabbitSender(_hostName, _vHost, _userName, _password, _queueName, _queueName);
        }

        IReceiver ITemporaryReceiverBuilder.Build()
        {
            return new RabbitTemporaryReceiver(_hostName, _vHost, _userName, _password, _queueName, _queueName, _startupTime, _messageWaitTimeout, _whenNoMoreMessages);

        }

        IReceiver IPermanentReceiverBuilder.Build()
        {
            return new RabbitPermanentReceiver(_hostName, _vHost, _userName, _password, _queueName, _queueName);
        }
    }
}
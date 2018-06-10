using HiQ.Implementations;
using HiQ.Interfaces;
using System;

namespace HiQ.Builders
{
    public enum Direction
    {
        Sender,
        Receiver,
    }
    public class RabbitBuilder : IQueueBuilder, IDirectionSelector, IReceiveTypeSelector, ISenderBuilder, ITemporaryReceiverBuilder, IPermanentReceiverBuilder
    {
        string _queueName;
        string _hostName;
        string _userName;
        string _password;
        Direction _direction;

        private TimeSpan _startupTime;
        private TimeSpan _messageWaitTimeout;
        private Action _whenNoMoreMessages;

        IDirectionSelector IQueueBuilder.ConfigureTransport(string hostName, string userName, string password)
        {
            _hostName = hostName;
            _userName = userName;
            _password = password;

            return this;
        }

        ISenderBuilder IDirectionSelector.ISendTo(string queueName)
        {
            _direction = Direction.Sender;

            _queueName = queueName;

            return this;
        }

        IReceiveTypeSelector IDirectionSelector.IReceiveFrom(string queueName)
        {
            _direction = Direction.Receiver;

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
            return new RabbitSender(_hostName, _userName, _password, _queueName, _queueName);
        }

        IReceiver ITemporaryReceiverBuilder.Build()
        {
            return new RabbitTemporaryReceiver(_hostName, _userName, _password, _queueName, _queueName, _startupTime, _messageWaitTimeout, _whenNoMoreMessages);

        }

        IReceiver IPermanentReceiverBuilder.Build()
        {
            return new RabbitPermanentReceiver(_hostName, _userName, _password, _queueName, _queueName);
        }
    }
}

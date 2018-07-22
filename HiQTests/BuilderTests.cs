using HiQ.Builders;
using HiQ.Implementations;
using HiQ.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HiQTests
{
    [TestFixture]
    public class BuilderTests
    {
        private string _hostName = "my-rabbit";
        private string _vHost = "";
        private string _userName = "remote";
        private string _password = "remote";
        private string _queueName = "BuilderTests";

        [Test]
        public void IQueueBuilder_returns_IDirectionSelector()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res = sut.ConfigureTransport(_hostName, _vHost, _userName, _password);

            Assert.IsInstanceOf<IDirectionSelector>(res);
        }

        [Test]
        public void ISenderBuilder_returns_ISenderBuilder()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .ISendTo(_queueName);

            Assert.IsInstanceOf<ISenderBuilder>(res);
        }

        [Test]
        public void ISenderBuilder_returns_ISender()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .ISendTo(_queueName)
                .Build();

            Assert.IsInstanceOf<RabbitSender>(res);
        }

        [Test]
        public void IReceiveFrom_returns_IReceiveTypeSelector()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .IReceiveFrom(_queueName);

            Assert.IsInstanceOf<IReceiveTypeSelector>(res);
        }

        [Test]
        public void IReceiveTypeSelector_returns_IPermanentReceiverBuilder()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .IReceiveFrom(_queueName)
                .IReceiveForever();

            Assert.IsInstanceOf<IPermanentReceiverBuilder>(res);
        }

        [Test]
        public void IPermanentReceiverBuilder_returns_RabbitPermanentReceiver()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .IReceiveFrom(_queueName)
                .IReceiveForever()
                .Build();

            Assert.IsInstanceOf<RabbitPermanentReceiver>(res);
        }

        [Test]
        public void IReceiveTypeSelector_returns_ITemporaryReceiverBuilder()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .IReceiveFrom(_queueName)
                .IReceiveUntilNoMoreMessages(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), () => { });

            Assert.IsInstanceOf<ITemporaryReceiverBuilder>(res);
        }

        [Test]
        public void ITemporaryReceiverBuilder_returns_TemporaryReceiver()
        {
            IQueueBuilder sut = new RabbitBuilder();

            var res =
                sut.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .IReceiveFrom(_queueName)
                .IReceiveUntilNoMoreMessages(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), () => { })
                .Build();

            Assert.IsInstanceOf<RabbitTemporaryReceiver>(res);
        }

       [Test]
        public async Task TestSendReceive()
        {
            IQueueBuilder builder;

            builder = new RabbitBuilder();
            var sender = builder.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .ISendTo(_queueName).Build();

            builder = new RabbitBuilder();
            var receiver = builder.ConfigureTransport(_hostName, _vHost, _userName, _password)
                .IReceiveFrom(_queueName).IReceiveForever().Build();

            String message = "Test Message";

            sender.Send<String>(message);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(2000);

            Func<string> subscriptionFunction = () =>
            {
                String result = null;
                receiver.Receive<String>((s) => { result = s; });
                while (string.IsNullOrEmpty(result))
                {
                    Task.Delay(100);
                }
                return result;
            };

            var res = await Task<string>.Run(subscriptionFunction, cancellationTokenSource.Token);

            Assert.AreEqual(message, res);
        }
    }
}
